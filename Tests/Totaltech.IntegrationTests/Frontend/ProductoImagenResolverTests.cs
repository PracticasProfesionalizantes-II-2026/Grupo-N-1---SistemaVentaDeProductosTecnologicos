using System.Net;
using System.Net.Http.Json;
using Frontend.Models.Api.Responses;
using Frontend.Services;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ProductoImagenResolverTests
{
    [Theory]
    [InlineData("asus")]
    [InlineData("Samsung")]
    [InlineData("Disco ssd")]
    [InlineData("Naxido")]
    [InlineData("Teclado")]
    [InlineData("NVIDIA G4050")]
    [InlineData("HP")]
    [InlineData("DELL")]
    [InlineData("LENOVO VICTUS")]
    [InlineData("asus vivobook")]
    [InlineData("Mouse")]
    public void NoInfiereModeloPorMarcaCategoriaONombreGenerico(string nombre)
    {
        Assert.Null(new ProductoImagenResolver().Resolver(nombre));
    }

    [Theory]
    [InlineData("Notebook Lenovo Yoga 7 2 en 1")]
    [InlineData("Auriculares HyperX Cloud Stinger")]
    [InlineData("ASUS PRIME GeForce RTX 5070")]
    public void ConservaCoincidenciaExactaConNormalizacionExistente(string nombre)
    {
        var resolver = new ProductoImagenResolver();
        var url = resolver.Resolver(nombre);
        Assert.NotNull(url);
        Assert.StartsWith("/images/categorias/", url);
        Assert.Equal(url, resolver.Resolver("  " + nombre.ToLowerInvariant() + "  "));
        Assert.Null(resolver.Resolver(nombre + " otro modelo"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListadoYCatalogoConservanUrlExplicitaYNoCruzanIdsEntreEntornos(bool catalogo)
    {
        const string imagen = "/uploads/productos/dcd308fe347a4dcc86a2d59fb9d50098.webp";
        using var client = new HttpClient(new ApiImagenes(imagen)) { BaseAddress = new Uri("https://api.test/") };
        var service = new ProductosApiService(new Factory(client), new ProductoImagenResolver());
        var productos = catalogo
            ? (await service.ObtenerCatalogoAsync(null, null, null, null, false, 1, 12)).Catalogo!.Items
            : await service.ObtenerTodosAsync();
        Assert.Equal(imagen, productos.Single(p => p.IdProducto == 10).ImagenUrl);
        Assert.Null(productos.Single(p => p.IdProducto == 23).ImagenUrl);
        Assert.Equal(new ProductoImagenResolver().Resolver("Notebook HP OMEN 14"),
            productos.Single(p => p.IdProducto == 100).ImagenUrl);
        Assert.Equal(imagen, productos.Single(p => p.IdProducto == 101).ImagenUrl);
    }

    private sealed class Factory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class ApiImagenes(string imagen) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var productos = new List<ProductoResponse>
            {
                new() { IdProducto = 10, Nombre = "asus vivobook", ImagenUrl = imagen },
                new() { IdProducto = 23, Nombre = "Mouse" },
                new() { IdProducto = 100, Nombre = "Notebook HP OMEN 14" },
                new() { IdProducto = 101, Nombre = "Notebook HP OMEN 14", ImagenUrl = imagen }
            };
            var content = request.RequestUri!.AbsolutePath == "/productos/catalogo"
                ? JsonContent.Create(new CatalogoProductosResponse { Items = productos })
                : JsonContent.Create(productos);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}
