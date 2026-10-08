using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.IntegrationTests.Endpoints;

public sealed class ProductosAdministracionEndpointsTests
{
    private const string Imagen = "/uploads/productos/0123456789abcdef0123456789abcdef.jpg";

    [Fact]
    public async Task Imagen_PersisteYSeDevuelveEnDetalleListadoYCatalogo()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        Autenticar(factory, client, RolUsuario.Administrador);
        var producto = await SembrarAsync(factory);

        using var actualizacion = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", Solicitud(producto, Imagen));
        Assert.Equal(HttpStatusCode.OK, actualizacion.StatusCode);
        Assert.Equal(Imagen, (await actualizacion.Content.ReadFromJsonAsync<Producto>())!.ImagenUrl);

        var detalle = await client.GetFromJsonAsync<Producto>($"/productos/{producto.IdProducto}");
        var listado = await client.GetFromJsonAsync<List<Producto>>("/productos/");
        var catalogo = await client.GetFromJsonAsync<CatalogoProductosResponse>("/productos/catalogo?texto=Producto%20imagen");
        Assert.Equal(Imagen, detalle!.ImagenUrl);
        Assert.Equal(Imagen, Assert.Single(listado!, p => p.IdProducto == producto.IdProducto).ImagenUrl);
        Assert.Equal(Imagen, Assert.Single(catalogo!.Items).ImagenUrl);
    }

    [Fact]
    public async Task ActualizarSinImagenOConNull_ConservaImagenAlRenombrar()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        Autenticar(factory, client, RolUsuario.Administrador);
        var producto = await SembrarAsync(factory, Imagen);
        using var omitida = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", new
        {
            nombre = "Nombre nuevo", producto.Descripcion, producto.Precio, producto.Stock,
            producto.IdCategoria, producto.IdProveedor
        });
        Assert.Equal(HttpStatusCode.OK, omitida.StatusCode);
        Assert.Equal(Imagen, (await omitida.Content.ReadFromJsonAsync<Producto>())!.ImagenUrl);

        using var nula = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", Solicitud(producto, null));
        Assert.Equal(HttpStatusCode.OK, nula.StatusCode);
        Assert.Equal(Imagen, (await nula.Content.ReadFromJsonAsync<Producto>())!.ImagenUrl);
    }

    [Theory]
    [InlineData("/uploads/productos/0123456789abcdef0123456789abcdef.png")]
    [InlineData("/uploads/productos/0123456789abcdef0123456789abcdef.webp")]
    [InlineData("/images/categorias/Notebooks/6 compragamer_Imganen_general_0_Notebook_Lenovo_LOQ_15AHP10_15.6__AM.jpg")]
    public async Task ActualizarImagen_AceptaSubidasYOriginalesSeguras(string imagen)
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        Autenticar(factory, client, RolUsuario.Administrador);
        var producto = await SembrarAsync(factory);
        using var respuesta = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", Solicitud(producto, imagen));
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(imagen, (await respuesta.Content.ReadFromJsonAsync<Producto>())!.ImagenUrl);
    }

    [Fact]
    public async Task ActualizarImagen_RechazaRutasNoPermitidasSinCambiarDatos()
    {
        string[] rutasInvalidas =
        [
            "", "https://sitio.test/foto.jpg", "//sitio.test/foto.jpg", "javascript:alert(1)",
            "/uploads/productos/foto.jpg", "/uploads/productos/0123456789abcdef0123456789abcdef.svg",
            "/uploads/productos/sub/0123456789abcdef0123456789abcdef.jpg",
            "/images/categorias/../foto.jpg", "/images/categorias/./foto.jpg",
            "/images/categorias//foto.jpg", "/images/categorias/%2e%2e/foto.jpg",
            "/images/categorias/foto.jpg?x=1", "/images/categorias/foto.jpg#fragmento",
            "/images/categorias\\foto.jpg", "/images/categorias/foto\n.jpg",
            "/images/categorias/foto.jpg:stream", "/images/categorias/ foto.jpg",
            "/images/categorias/" + new string('a', 480) + ".jpg"
        ];
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        Autenticar(factory, client, RolUsuario.Administrador);
        var producto = await SembrarAsync(factory, Imagen);
        foreach (var ruta in rutasInvalidas)
        {
            var request = Solicitud(producto, ruta);
            request.Nombre = "No debe guardarse";
            using var respuesta = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", request);
            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            var actual = await client.GetFromJsonAsync<Producto>($"/productos/{producto.IdProducto}");
            Assert.Equal(producto.Nombre, actual!.Nombre);
            Assert.Equal(Imagen, actual.ImagenUrl);
        }
    }

    [Fact]
    public async Task CrearProducto_SinImagenConservaContratoYConImagenLaValida()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        Autenticar(factory, client, RolUsuario.Administrador);
        var producto = await SembrarAsync(factory);
        using var anterior = await client.PostAsJsonAsync("/productos/", Solicitud(producto, null));
        Assert.Equal(HttpStatusCode.Created, anterior.StatusCode);
        Assert.Null((await anterior.Content.ReadFromJsonAsync<Producto>())!.ImagenUrl);
        using var conImagen = await client.PostAsJsonAsync("/productos/", Solicitud(producto, Imagen));
        Assert.Equal(HttpStatusCode.Created, conImagen.StatusCode);
        Assert.Equal(Imagen, (await conImagen.Content.ReadFromJsonAsync<Producto>())!.ImagenUrl);
        using var invalida = await client.PostAsJsonAsync("/productos/", Solicitud(producto, "https://invalid.test/foto.jpg"));
        Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);
    }

    [Fact]
    public async Task ActualizarYEliminar_ExigenAdministrador()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        var producto = await SembrarAsync(factory, Imagen);
        using var anonimoPut = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", Solicitud(producto, Imagen));
        using var anonimoDelete = await client.DeleteAsync($"/productos/{producto.IdProducto}");
        Assert.Equal(HttpStatusCode.Unauthorized, anonimoPut.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonimoDelete.StatusCode);
        Autenticar(factory, client, RolUsuario.Cliente);
        using var clientePut = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", Solicitud(producto, Imagen));
        using var clienteDelete = await client.DeleteAsync($"/productos/{producto.IdProducto}");
        Assert.Equal(HttpStatusCode.Forbidden, clientePut.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, clienteDelete.StatusCode);
        Assert.NotNull(await client.GetFromJsonAsync<Producto>($"/productos/{producto.IdProducto}"));
    }

    [Fact]
    public async Task EliminarSinRelaciones_BorraProductoYRepetirDevuelveNotFound()
    {
        await using var factory = new TotaltechWebApplicationFactory();
        using var client = factory.CreateClient();
        Autenticar(factory, client, RolUsuario.Administrador);
        var producto = await SembrarAsync(factory, Imagen);
        using var eliminacion = await client.DeleteAsync($"/productos/{producto.IdProducto}");
        using var repetida = await client.DeleteAsync($"/productos/{producto.IdProducto}");
        using var editar = await client.PutAsJsonAsync($"/productos/{producto.IdProducto}", Solicitud(producto, Imagen));
        Assert.Equal(HttpStatusCode.NoContent, eliminacion.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, repetida.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, editar.StatusCode);
    }

    private static void Autenticar(TotaltechWebApplicationFactory factory, HttpClient client, RolUsuario rol)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var usuario = new Usuario { Nombre = "Prueba", Email = $"imagen-{Guid.NewGuid():N}@test.local", Rol = rol };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
        var token = factory.Services.GetRequiredService<IJwtTokenService>().Crear(usuario);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    private static ProductoRequest Solicitud(Producto producto, string? imagen) => new()
    {
        Nombre = producto.Nombre, Descripcion = producto.Descripcion, Precio = producto.Precio,
        Stock = producto.Stock, IdCategoria = producto.IdCategoria, IdProveedor = producto.IdProveedor,
        ImagenUrl = imagen
    };

    private static async Task<Producto> SembrarAsync(TotaltechWebApplicationFactory factory, string? imagen = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TotaltechDbContext>();
        var producto = new Producto
        {
            Nombre = "Producto imagen", Descripcion = "Producto para probar administración", Precio = 150,
            Stock = 3, ImagenUrl = imagen,
            Categoria = new Categoria { Nombre = "Categoría imagen" },
            Proveedor = new Proveedor { RazonSocial = "Proveedor imagen", Cuit = "30-12345678-9", EmailComercial = "imagen@test.local" }
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }
}
