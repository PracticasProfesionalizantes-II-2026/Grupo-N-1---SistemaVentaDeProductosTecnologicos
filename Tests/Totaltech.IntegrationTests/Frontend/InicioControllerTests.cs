using System.Net;
using System.Net.Http.Json;
using Frontend.Controllers;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Home;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class InicioControllerTests
{
    [Fact]
    public async Task InicioConsultaUnaVezYConservaTodosLosPreciosYAgotados()
    {
        var api = new ProductosPrueba();
        using var client = new HttpClient(api) { BaseAddress = new Uri("https://api.test") };
        var factory = new Factory(client);
        var controller = new HomeController(factory, NullLogger<HomeController>.Instance,
            new ProductosApiService(factory, new ProductoImagenResolver()), new CategoriasApiService(factory));
        var view = Assert.IsType<ViewResult>(await controller.Index(default));
        var model = Assert.IsType<InicioViewModel>(view.Model);
        Assert.Equal(1, api.Lecturas);
        Assert.Equal(1, api.CategoriasLeidas);
        Assert.Contains(model.Categorias, c => c.IdCategoria == 99);
        Assert.Equal(new[] { 2, 3, 1 }, model.Productos.Select(p => p.IdProducto));
        Assert.Equal(123.45m, model.Productos.Single(p => p.IdProducto == 1).Precio);
        Assert.Contains(model.Productos, p => p.Stock == 0);
        Assert.DoesNotContain(model.Destacados, p => p.Stock == 0);
    }

    [Fact]
    public void DestacadosSonDiversosEstablesYNoDuplicados()
    {
        var productos = Enumerable.Range(1, 10).Select(i => new ProductoResponse
        { IdProducto = i, Nombre = "Producto " + i.ToString("D2"), Stock = 1, IdCategoria = (i % 3) + 1 }).ToArray();
        var first = InicioViewModel.Desde(productos);
        var second = InicioViewModel.Desde(productos.Reverse());
        Assert.Equal(6, first.Destacados.Count);
        Assert.Equal(6, first.Destacados.Select(p => p.IdProducto).Distinct().Count());
        Assert.Equal(3, first.Destacados.Take(3).Select(p => p.IdCategoria).Distinct().Count());
        Assert.Equal(first.Destacados.Select(p => p.IdProducto), second.Destacados.Select(p => p.IdProducto));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void NoSeInventanProductosParaCompletarDestacados(int cantidad)
    {
        var model = InicioViewModel.Desde(Enumerable.Range(1, cantidad).Select(i => new ProductoResponse
        { IdProducto = i, Nombre = "Producto", Stock = 1 }));
        Assert.Equal(cantidad, model.Productos.Count);
        Assert.Equal(cantidad, model.Destacados.Count);
    }

    [Theory]
    [InlineData("red")]
    [InlineData("timeout")]
    [InlineData("json")]
    public async Task FallosDeApiMuestranErrorSinProductosInventados(string modo)
    {
        using var client = new HttpClient(new ProductosPrueba { Modo = modo }) { BaseAddress = new Uri("https://api.test") };
        var factory = new Factory(client);
        var controller = new HomeController(factory, NullLogger<HomeController>.Instance,
            new ProductosApiService(factory, new ProductoImagenResolver()), new CategoriasApiService(factory));
        var view = Assert.IsType<ViewResult>(await controller.Index(default));
        var model = Assert.IsType<InicioViewModel>(view.Model);
        Assert.NotNull(model.Error);
        Assert.Empty(model.Productos);
        Assert.Empty(model.Destacados);
        Assert.NotEmpty(model.Categorias);
        Assert.Null(model.ErrorCategorias);
    }

    [Fact]
    public async Task CancelacionSolicitadaNoSeConvierteEnErrorDeApi()
    {
        using var client = new HttpClient(new ProductosPrueba()) { BaseAddress = new Uri("https://api.test") };
        var factory = new Factory(client);
        var controller = new HomeController(factory, NullLogger<HomeController>.Instance,
            new ProductosApiService(factory, new ProductoImagenResolver()), new CategoriasApiService(factory));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => controller.Index(cancellation.Token));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("timeout")]
    [InlineData("json")]
    public async Task ErrorDeCategoriasConservaProductosYDestacados(string modo)
    {
        var api = new ProductosPrueba { ModoCategorias = modo };
        using var client = new HttpClient(api) { BaseAddress = new Uri("https://api.test") };
        var factory = new Factory(client);
        var controller = new HomeController(factory, NullLogger<HomeController>.Instance,
            new ProductosApiService(factory, new ProductoImagenResolver()), new CategoriasApiService(factory));
        var model = Assert.IsType<InicioViewModel>(Assert.IsType<ViewResult>(await controller.Index(default)).Model);
        Assert.NotNull(model.ErrorCategorias);
        Assert.Null(model.Error);
        Assert.Equal(3, model.Productos.Count);
        Assert.NotEmpty(model.Destacados);
        Assert.Equal(1, api.Lecturas);
        Assert.Equal(1, api.CategoriasLeidas);
    }

    [Theory]
    [InlineData("CELULARES", "celulares.jpg")]
    [InlineData("periféricos", "perifericos.jpg")]
    [InlineData("  PERIFERICOS  ", "perifericos.jpg")]
    [InlineData("Notebooks", "notebooks.jpg")]
    [InlineData("Monitores", "monitores.jpg")]
    [InlineData("Categoría nueva", null)]
    public void IlustracionesDependenDelNombreRealSinFijarIds(string nombre, string? archivo)
    {
        var tarjeta = InicioCategoriaViewModel.Desde(new CategoriaResponse { IdCategoria = 87, Nombre = nombre });
        Assert.Equal(87, tarjeta.IdCategoria);
        Assert.Equal(nombre, tarjeta.Nombre);
        Assert.Equal(archivo is null ? null : "~/images/categorias/" + archivo, tarjeta.ImagenUrl);
    }

    [Fact]
    public void CategoriasSeOrdenanEstablementeSinInventarMonitores()
    {
        CategoriaResponse[] categorias = [new() { IdCategoria = 8, Nombre = "Zeta" },
            new() { IdCategoria = 6, Nombre = "Álmacenamiento" }, new() { IdCategoria = 3, Nombre = "Álmacenamiento" }];
        var first = InicioViewModel.Desde([], categorias);
        var second = InicioViewModel.Desde([], categorias.Reverse());
        Assert.Equal(new[] { 3, 6, 8 }, first.Categorias.Select(c => c.IdCategoria));
        Assert.Equal(first.Categorias.Select(c => c.IdCategoria), second.Categorias.Select(c => c.IdCategoria));
        Assert.DoesNotContain(first.Categorias, c => c.Nombre == "Monitores");
        Assert.Empty(InicioViewModel.Desde([], []).Categorias);
    }

    private sealed class Factory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class ProductosPrueba : HttpMessageHandler
    {
        public int Lecturas { get; private set; }
        public int CategoriasLeidas { get; private set; }
        public string Modo { get; init; } = "";
        public string ModoCategorias { get; init; } = "";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            Assert.Equal(HttpMethod.Get, request.Method);
            if (request.RequestUri!.AbsolutePath == "/categorias/")
            {
                CategoriasLeidas++;
                if (ModoCategorias == "red") throw new HttpRequestException();
                if (ModoCategorias == "timeout") throw new TaskCanceledException();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = ModoCategorias == "json" ? new StringContent("invalid", System.Text.Encoding.UTF8, "application/json") :
                        JsonContent.Create(new[] { new CategoriaResponse { IdCategoria = 99, Nombre = "Monitores" } })
                });
            }
            Lecturas++;
            Assert.Equal("/productos/", request.RequestUri!.AbsolutePath);
            if (Modo == "red") throw new HttpRequestException();
            if (Modo == "timeout") throw new TaskCanceledException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = Modo == "json" ? new StringContent("invalid", System.Text.Encoding.UTF8, "application/json") :
                    JsonContent.Create(new[]
                    {
                        new ProductoResponse { IdProducto = 1, Nombre = "Zeta", Precio = 123.45m, Stock = 4, IdCategoria = 1 },
                        new ProductoResponse { IdProducto = 3, Nombre = "Alfa", Precio = 42m, Stock = 0, IdCategoria = 2 },
                        new ProductoResponse { IdProducto = 2, Nombre = "Alfa", Precio = 10m, Stock = 3, IdCategoria = 2 }
                    })
            });
        }
    }
}
