using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Frontend.Controllers;
using Frontend.Models.Api.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class InicioHttpTests
{
    [Fact]
    public async Task InicioPublicoMuestraTodosLosProductosYEstadosSinAutenticar()
    {
        var api = new ApiPrueba();
        await using var factory = Factory(api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
        var html = await client.GetStringAsync("/");
        Assert.Contains("Tecnología para tu próximo proyecto", html);
        Assert.Contains("Todos nuestros productos", html);
        Assert.Contains("Sin stock", html);
        Assert.Contains("Imagen no disponible", html);
        Assert.Contains("Iniciar sesión", html);
        Assert.DoesNotContain("Cerrar sesión", html);
        Assert.Equal(1, api.ProductosLeidos);
        Assert.Equal(1, api.CategoriasLeidas);
        Assert.Contains("Explorá por categoría", html);
        Assert.Contains("href=\"/Productos?idCategoria=99\"", html);
        Assert.Contains("data-categoria-id=\"99\"", html);
        Assert.Contains("/images/categorias/perifericos.jpg", html);
        Assert.Contains("Categoría nueva", WebUtility.HtmlDecode(html));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LogoutBorraCookieConAntiforgeryInclusoSinApi(bool sinApi)
    {
        var api = new ApiPrueba();
        await using var factory = Factory(api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test"), AllowAutoRedirect = false });
        await LoginAsync(client);
        var html = await client.GetStringAsync("/");
        Assert.Contains("Cerrar sesión", html);
        Assert.Contains("class=\"header-logout\"", html);
        Assert.DoesNotContain("navigation-logout", html);
        Assert.True(html.IndexOf("class=\"cart-link\"", StringComparison.Ordinal) < html.IndexOf("class=\"header-logout\"", StringComparison.Ordinal));
        Assert.DoesNotContain("Cerrar sesión", Regex.Match(html, "<nav class=\"main-navigation\"[\\s\\S]*?</nav>").Value);
        Assert.DoesNotContain(">Iniciar sesión</a>", html);
        var llamadasAntes = api.Llamadas;
        api.SinApi = sinApi;
        using var response = await client.PostAsync("/Home/Logout", Form(("__RequestVerificationToken", Token(html))));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("expires=", response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("Totaltech.Auth=")));
        Assert.Equal(llamadasAntes, api.Llamadas);
        api.SinApi = false;
        var anonimo = await client.GetStringAsync("/");
        Assert.Contains(">Iniciar sesión</a>", anonimo);
        Assert.DoesNotContain("Cerrar sesión", anonimo);
        using var protegido = await client.GetAsync("/Carrito");
        Assert.Equal(HttpStatusCode.Redirect, protegido.StatusCode);
        Assert.Contains("/Home/Login", protegido.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task LogoutSinTokenOConGetNoCierraSesion()
    {
        var api = new ApiPrueba();
        await using var factory = Factory(api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test"), AllowAutoRedirect = false });
        await LoginAsync(client);
        using var post = await client.PostAsync("/Home/Logout", Form());
        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        using var get = await client.GetAsync("/Home/Logout");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Contains("Cerrar sesión", await client.GetStringAsync("/"));
    }

    [Theory]
    [InlineData("vacio")]
    [InlineData("error")]
    public async Task InicioVacioOApiNoDisponibleConservaCabecera(string modo)
    {
        var api = new ApiPrueba { Vacio = modo == "vacio", SinApi = modo == "error" };
        await using var factory = Factory(api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
        var html = await client.GetStringAsync("/");
        Assert.Contains("Tecnología para tu próximo proyecto", html);
        Assert.DoesNotContain("data-producto-id=", html);
        Assert.Contains(modo == "vacio" ? "Pronto vas a encontrar" : "Reintentar", html);
    }

    [Theory]
    [InlineData("categorias")]
    [InlineData("productos")]
    public async Task FalloParcialMuestraElOtroListado(string fallo)
    {
        var api = new ApiPrueba { ErrorCategorias = fallo == "categorias", ErrorProductos = fallo == "productos" };
        await using var factory = Factory(api);
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
        var html = await client.GetStringAsync("/");
        Assert.Contains("Reintentar", html);
        if (fallo == "categorias")
        {
            Assert.Contains("data-inicio-productos", html);
            Assert.DoesNotContain("data-inicio-categorias", html);
        }
        else
        {
            Assert.Contains("data-inicio-categorias", html);
            Assert.DoesNotContain("data-inicio-productos", html);
        }
    }

    [Fact]
    public async Task CategoriasVaciasTienenMensajeSinTarjetasInventadas()
    {
        await using var factory = Factory(new ApiPrueba { CategoriasVacias = true });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://frontend.test") });
        var html = await client.GetStringAsync("/");
        Assert.Contains("Todavía no hay categorías disponibles", html);
        Assert.DoesNotContain("data-inicio-categorias", html);
        Assert.Contains("data-inicio-productos", html);
    }

    private static WebApplicationFactory<HomeController> Factory(ApiPrueba api) =>
        new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseStaticWebAssets();
            builder.ConfigureServices(services => services.AddSingleton<IHttpClientFactory>(api));
        });

    private static async Task LoginAsync(HttpClient client)
    {
        var html = await client.GetStringAsync("/Home/Login");
        using var response = await client.PostAsync("/Home/Login", Form(
            ("__RequestVerificationToken", Token(html)), ("Email", "cliente@test.local"), ("Contrasena", "ClaveDePrueba123")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static string Token(string html)
    {
        var match = Regex.Match(html, "name=\\\"__RequestVerificationToken\\\"[^>]*value=\\\"([^\\\"]+)\\\"");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] campos) =>
        new(campos.Select(c => new KeyValuePair<string, string>(c.Key, c.Value)));

    private sealed class ApiPrueba : HttpMessageHandler, IHttpClientFactory
    {
        public int Llamadas { get; private set; }
        public int ProductosLeidos { get; private set; }
        public int CategoriasLeidas { get; private set; }
        public bool SinApi { get; set; }
        public bool Vacio { get; init; }
        public bool CategoriasVacias { get; init; }
        public bool ErrorCategorias { get; init; }
        public bool ErrorProductos { get; init; }
        public HttpClient CreateClient(string name) => new(this, disposeHandler: false) { BaseAddress = new Uri("https://api.test") };
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Llamadas++;
            if (SinApi) throw new HttpRequestException("API de prueba no disponible.");
            object contenido = request.RequestUri!.AbsolutePath.TrimEnd('/') switch
            {
                "/auth/login" => new { IdUsuario = 7, Nombre = "Cliente", Apellido = "Prueba", Email = "cliente@test.local", Rol = 0, AccessToken = "token-ficticio", ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1) },
                "/usuarios/7" => new UsuarioResponse { IdUsuario = 7, Nombre = "Cliente", Apellido = "Prueba", Email = "cliente@test.local", Rol = 0, Activo = true },
                "/carritos/actual" => new CarritoResumenResponse(),
                "/productos" => Productos(),
                "/categorias" => Categorias(),
                _ => throw new InvalidOperationException("Ruta inesperada: " + request.RequestUri.AbsolutePath)
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(contenido, contenido.GetType()) });
        }
        private ProductoResponse[] Productos()
        {
            ProductosLeidos++;
            if (ErrorProductos) throw new HttpRequestException();
            return Vacio ? [] : [
                new() { IdProducto = 1, Nombre = "Producto disponible", Descripcion = "Descripción", Precio = 123m, Stock = 4, IdCategoria = 1 },
                new() { IdProducto = 2, Nombre = "Producto agotado", Descripcion = "Descripción", Precio = 456m, Stock = 0, IdCategoria = 2 }
            ];
        }
        private CategoriaResponse[] Categorias()
        {
            CategoriasLeidas++;
            if (ErrorCategorias) throw new HttpRequestException();
            return CategoriasVacias ? [] : [new() { IdCategoria = 99, Nombre = "Monitores" },
                new() { IdCategoria = 5, Nombre = "PERIFERICOS" }, new() { IdCategoria = 80, Nombre = "Categoría nueva" }];
        }
    }
}
