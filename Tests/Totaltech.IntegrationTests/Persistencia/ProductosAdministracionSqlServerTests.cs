using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;
using Totaltech.Seguridad;

namespace Totaltech.IntegrationTests.Persistencia;

[Trait("Category", "SqlServer")]
public sealed class ProductosAdministracionSqlServerTests : IClassFixture<SqlServerTestDatabase>
{
    private const string Imagen = "/uploads/productos/0123456789abcdef0123456789abcdef.jpg";
    private readonly SqlServerTestDatabase _database;

    public ProductosAdministracionSqlServerTests(SqlServerTestDatabase database) => _database = database;

    [Fact]
    public async Task Migracion_AgregaColumnaNullableSinCambiarProductosExistentes()
    {
        var database = new SqlServerTestDatabase();
        try
        {
            await using (var anterior = database.CreateContext())
            {
                await anterior.GetService<IMigrator>().MigrateAsync("20260914231145_InicialActualizada");
                var categoria = new Categoria { Nombre = "Categoría anterior" };
                var proveedor = new Proveedor { RazonSocial = "Proveedor anterior" };
                anterior.AddRange(categoria, proveedor);
                await anterior.SaveChangesAsync();
                await anterior.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, IdCategoria, IdProveedor)
                    VALUES ({"Producto anterior"}, {"Descripción anterior"}, {123.45m}, {7}, {categoria.IdCategoria}, {proveedor.IdProveedor})
                    """);
                await anterior.Database.MigrateAsync();
            }

            await using var actual = database.CreateContext();
            var producto = await actual.Productos.SingleAsync();
            Assert.Equal("Producto anterior", producto.Nombre);
            Assert.Equal("Descripción anterior", producto.Descripcion);
            Assert.Equal(123.45m, producto.Precio);
            Assert.Equal(7, producto.Stock);
            Assert.Null(producto.ImagenUrl);
            Assert.Empty(await actual.Database.GetPendingMigrationsAsync());

            var columna = await actual.Database.SqlQueryRaw<ColumnaImagen>("""
                SELECT IS_NULLABLE AS EsNullable, CHARACTER_MAXIMUM_LENGTH AS Longitud
                FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'ImagenUrl'
                """).SingleAsync();
            Assert.Equal("YES", columna.EsNullable);
            Assert.Equal(500, columna.Longitud);
        }
        finally
        {
            await database.DisposeAsync();
        }
    }

    [Fact]
    public async Task Imagen_PersisteEntreContextosYSeProyectaEnCatalogoSql()
    {
        int id;
        await using (var context = _database.CreateContext())
        {
            var producto = CrearProducto();
            context.Productos.Add(producto);
            await context.SaveChangesAsync();
            id = producto.IdProducto;
        }

        await using var nuevoContexto = _database.CreateContext();
        var persistido = await nuevoContexto.Productos.FindAsync(id);
        Assert.Equal(Imagen, persistido!.ImagenUrl);
        var catalogo = await new ProductosRepositorio(nuevoContexto).ObtenerCatalogoAsync(new FiltroCatalogoProductos
        {
            IdCategoria = persistido.IdCategoria
        });
        Assert.Equal(Imagen, Assert.Single(catalogo.Items).ImagenUrl);
        persistido.ImagenUrl = new string('a', 501);
        await Assert.ThrowsAsync<DbUpdateException>(() => nuevoContexto.SaveChangesAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EliminarProductoConCarritoOPedido_DevuelveConflictYConservaDatos(bool conPedido)
    {
        int id;
        await using (var context = _database.CreateContext())
        {
            var producto = CrearProducto();
            if (conPedido)
            {
                context.DetallePedidos.Add(new DetallePedido
                {
                    Producto = producto, Cantidad = 1, PrecioUnitario = producto.Precio, Subtotal = producto.Precio,
                    Pedido = new Pedido
                    {
                        FechaPedido = DateTime.UtcNow, Total = producto.Precio,
                        Direccion = new Direccion { Calle = "Calle prueba", Numero = "1" }
                    }
                });
            }
            else
            {
                context.DetalleCarritos.Add(new DetalleCarrito
                {
                    Producto = producto, Cantidad = 1, PrecioUnitario = producto.Precio, Subtotal = producto.Precio,
                    Carrito = new Carrito
                    {
                        FechaCreacion = DateTime.UtcNow,
                        Usuario = new Usuario
                        {
                            Nombre = "Prueba", Email = $"producto-{Guid.NewGuid():N}@test.local",
                            Contrasena = "hash-prueba", FechaRegistro = DateTime.UtcNow
                        }
                    }
                });
            }
            await context.SaveChangesAsync();
            id = producto.IdProducto;
        }

        await using var factory = new ProductosSqlFactory(_database.ConnectionString);
        using var client = factory.CreateClient();
        var token = factory.Services.GetRequiredService<IJwtTokenService>().Crear(new Usuario
        {
            IdUsuario = 100, Nombre = "Prueba", Email = "admin@test.local", Rol = RolUsuario.Administrador
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        using var respuesta = await client.DeleteAsync($"/productos/{id}");
        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("datos relacionados", await respuesta.Content.ReadAsStringAsync());
        var actual = await client.GetFromJsonAsync<Producto>($"/productos/{id}");
        Assert.Equal(Imagen, actual!.ImagenUrl);
        await using var verificacion = _database.CreateContext();
        Assert.True(conPedido
            ? await verificacion.DetallePedidos.AnyAsync(d => d.IdProducto == id)
            : await verificacion.DetalleCarritos.AnyAsync(d => d.IdProducto == id));
    }

    private static Producto CrearProducto() => new()
    {
        Nombre = "Producto imagen SQL", Descripcion = "Prueba aislada", ImagenUrl = Imagen,
        Precio = 150, Stock = 3, Categoria = new Categoria { Nombre = "Categoría imagen SQL" },
        Proveedor = new Proveedor { RazonSocial = "Proveedor imagen SQL" }
    };

    private sealed class ColumnaImagen
    {
        public string EsNullable { get; set; } = string.Empty;
        public int Longitud { get; set; }
    }

    private sealed class ProductosSqlFactory(string connectionString) : WebApplicationFactory<TotaltechDbContext>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            SqlServerTestDatabase.ValidateConnectionString(connectionString);
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
            builder.UseSetting("Authentication:Issuer", "Totaltech.SqlTests");
            builder.UseSetting("Authentication:Audience", "Totaltech.SqlTests");
            builder.UseSetting("Authentication:SigningKey", "sql-integration-tests-signing-key-2026");
            builder.UseSetting("Database:ApplyMigrations", "false");
            builder.UseSetting("DemoData:Enabled", "false");
            builder.UseSetting("BootstrapAdmin:Enabled", "false");
        }
    }
}
