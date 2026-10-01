using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.IntegrationTests.Persistencia;

[Trait("Category", "SqlServer")]
public sealed class CatalogoSqlServerTests
{
    [Fact]
    public async Task ConsultaCatalogo_TraduceFiltrosProyeccionConteoYPaginacionEnSqlServer()
    {
        var database = new SqlServerTestDatabase();
        await database.InitializeAsync();
        try
        {
            await using var context = database.CreateContext();
            var categoria = new Categoria { Nombre = "SQL Notebooks", Descripcion = "Prueba relacional" };
            var proveedor = new Proveedor
            {
                RazonSocial = "SQL Proveedor", Cuit = "30-12345678-9",
                EmailComercial = "sql@test.local", TelefonoComercial = "123",
                CondicionIva = "RI", MonedaPreferida = "ARS", Activo = true
            };
            context.AddRange(categoria, proveedor);
            await context.SaveChangesAsync();
            context.Productos.AddRange(
                new Producto { Nombre = "Equipo repetido", Descripcion = "Notebook SQL uno", Precio = 100, Stock = 2, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor },
                new Producto { Nombre = "Equipo repetido", Descripcion = "Notebook SQL dos", Precio = 100, Stock = 4, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor },
                new Producto { Nombre = "Equipo sin stock", Descripcion = "Notebook SQL", Precio = 100, Stock = 0, IdCategoria = categoria.IdCategoria, IdProveedor = proveedor.IdProveedor });
            await context.SaveChangesAsync();

            var repositorio = new ProductosRepositorio(context);
            var primera = await repositorio.ObtenerCatalogoAsync(new FiltroCatalogoProductos
            {
                Texto = "Notebook SQL", IdCategoria = categoria.IdCategoria,
                PrecioMin = 100, PrecioMax = 100, SoloDisponibles = true,
                Pagina = 1, TamanoPagina = 1
            });
            var segunda = await repositorio.ObtenerCatalogoAsync(new FiltroCatalogoProductos
            {
                Texto = "Notebook SQL", IdCategoria = categoria.IdCategoria,
                PrecioMin = 100, PrecioMax = 100, SoloDisponibles = true,
                Pagina = 2, TamanoPagina = 1
            });

            Assert.Equal(2, primera.TotalItems);
            Assert.Equal(2, primera.TotalPaginas);
            Assert.Equal("SQL Notebooks", Assert.Single(primera.Items).CategoriaNombre);
            Assert.Equal("SQL Proveedor", primera.Items[0].ProveedorNombre);
            Assert.NotEqual(primera.Items[0].IdProducto, Assert.Single(segunda.Items).IdProducto);
            Assert.True(primera.Items[0].IdProducto < segunda.Items[0].IdProducto);
        }
        finally
        {
            await database.DisposeAsync();
        }
    }
}
