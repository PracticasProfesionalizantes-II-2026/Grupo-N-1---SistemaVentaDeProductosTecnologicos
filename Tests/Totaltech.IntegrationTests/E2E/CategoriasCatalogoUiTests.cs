using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Frontend.Controllers;
using Frontend.Models.Api.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

public sealed class CategoriasCatalogoUiTests
{
    [Fact]
    public async Task VistaReal_ConservaFiltrosCategoriaPaginaYSelectorAdmin_EnSeisAnchos()
    {
        // MVC y Razor reales en TestServer; la API se sustituye sin usar bases de datos.
        using var factory = new WebApplicationFactory<ProductosController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IHttpClientFactory, CatalogoApi>();
                services.AddTransient<IStartupFilter, UsuarioAdmin>();
            });
        });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://catalogo.test") });
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.RouteAsync("https://catalogo.test/**", async route =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, route.Request.Url);
            if (route.Request.Headers.TryGetValue("x-test-admin", out var admin))
                request.Headers.Add("X-Test-Admin", admin);
            using var response = await client.SendAsync(request);
            await route.FulfillAsync(new()
            {
                Status = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream",
                BodyBytes = await response.Content.ReadAsByteArrayAsync()
            });
        });
        await page.GotoAsync("https://catalogo.test/Productos?texto=Producto&precioMin=10&precioMax=1000&soloDisponibles=true&tamanoPagina=24&pagina=2");
        Assert.Contains("catalog-categories", await page.ContentAsync());
        Assert.Contains("Notebooks", await page.ContentAsync());
        var categories = page.Locator(".catalog-categories");
        await page.SetViewportSizeAsync(1280, 900);
        await page.Locator(".catalog-categories__list a", new() { HasTextString = "Notebooks" }).ClickAsync();
        var query = QueryHelpers.ParseQuery(new Uri(page.Url).Query);
        Assert.Equal("1", query["idCategoria"]);
        Assert.False(query.ContainsKey("pagina"));
        Assert.Equal("Producto", query["texto"]);
        Assert.Equal("10", query["precioMin"]);
        Assert.Equal("1000", query["precioMax"]);
        Assert.Equal("true", query["soloDisponibles"]);
        Assert.Equal("24", query["tamanoPagina"]);
        Assert.Equal("Notebooks", await page.Locator(".catalog-categories a[aria-current=page]").InnerTextAsync());
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar", Exact = true }).ClickAsync();
        Assert.Equal("1", QueryHelpers.ParseQuery(new Uri(page.Url).Query)["idCategoria"]);
        await page.Locator(".pagination a", new() { HasTextString = "2" }).ClickAsync();
        query = QueryHelpers.ParseQuery(new Uri(page.Url).Query);
        Assert.Equal("1", query["idCategoria"]);
        Assert.Equal("2", query["pagina"]);

        foreach (var width in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(width, 900);
            await page.WaitForFunctionAsync("w => document.querySelector('summary').tabIndex === (w >= 768 ? -1 : 0)", width);
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= window.innerWidth"));
            Assert.Equal(width >= 1200 ? 3 : width >= 768 ? 2 : 1,
                await page.Locator(".catalogo__grilla").EvaluateAsync<int>("e => getComputedStyle(e).gridTemplateColumns.split(' ').length"));
            var sidebar = await page.Locator(".catalogo__sidebar").BoundingBoxAsync();
            var content = await page.Locator(".catalogo__contenido").BoundingBoxAsync();
            Assert.NotNull(sidebar);
            Assert.NotNull(content);
            if (width >= 768)
            {
                Assert.True(sidebar!.X + sidebar.Width <= content!.X + 1);
                Assert.True(await categories.EvaluateAsync<bool>("e => e.open"));
                await categories.Locator("summary").ClickAsync();
                Assert.True(await categories.EvaluateAsync<bool>("e => e.open"));
            }
            else
            {
                Assert.True(sidebar!.Y + sidebar.Height <= content!.Y + 1);
                await categories.Locator("summary").FocusAsync();
                var wasOpen = await categories.EvaluateAsync<bool>("e => e.open");
                await page.Keyboard.PressAsync("Enter");
                Assert.Equal(!wasOpen, await categories.EvaluateAsync<bool>("e => e.open"));
                await page.Keyboard.PressAsync("Space");
                Assert.Equal(wasOpen, await categories.EvaluateAsync<bool>("e => e.open"));
            }
            var output = Path.Combine(AppContext.BaseDirectory, "TestResults", "categorias");
            Directory.CreateDirectory(output);
            await page.ScreenshotAsync(new() { Path = Path.Combine(output, $"catalogo-{width}.png"), FullPage = true });
        }
        await page.Locator(".catalog-categories a", new() { HasTextString = "Todas" }).ClickAsync();
        Assert.False(QueryHelpers.ParseQuery(new Uri(page.Url).Query).ContainsKey("idCategoria"));
        Assert.Equal("Todas", await page.Locator(".catalog-categories a[aria-current=page]").InnerTextAsync());
        await page.Locator("#texto").FillAsync("sin resultados");
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar", Exact = true }).ClickAsync();
        Assert.Contains("No hay productos", await page.Locator(".catalogo__contenido").InnerTextAsync());
        Assert.Equal(3, await page.Locator(".catalog-categories a").CountAsync());

        await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string> { ["X-Test-Admin"] = "true" });
        await page.GotoAsync("https://catalogo.test/Productos?idCategoria=1");
        Assert.Equal(0, await page.Locator(".catalog-categories").CountAsync());
        Assert.Equal("1", await page.Locator("select#idCategoria").InputValueAsync());
        await page.Locator("select#idCategoria").SelectOptionAsync("2");
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar", Exact = true }).ClickAsync();
        Assert.Equal("2", QueryHelpers.ParseQuery(new Uri(page.Url).Query)["idCategoria"]);
    }

    private sealed class CatalogoApi : HttpMessageHandler, IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(this, false) { BaseAddress = new Uri("https://api.test") };
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var categories = new[] {
                new CategoriaResponse { IdCategoria = 1, Nombre = "Notebooks" },
                new CategoriaResponse { IdCategoria = 2, Nombre = "Almacenamiento externo" }
            };
            object data;
            if (request.RequestUri!.AbsolutePath.StartsWith("/categorias"))
                data = categories;
            else
            {
                var query = QueryHelpers.ParseQuery(request.RequestUri.Query);
                var empty = query.TryGetValue("texto", out var text) && text == "sin resultados";
                var category = query.TryGetValue("idCategoria", out var categoryValue) && int.TryParse(categoryValue, out var id) ? id : 1;
                data = new CatalogoProductosResponse
                {
                    Pagina = int.TryParse(query["pagina"], out var number) ? number : 1,
                    TamanoPagina = int.TryParse(query["tamanoPagina"], out var size) ? size : 12,
                    TotalItems = empty ? 0 : 50,
                    TotalPaginas = empty ? 0 : 3,
                    Items = empty ? [] : Enumerable.Range(1, 6).Select(i => new ProductoResponse
                    {
                        IdProducto = i, Nombre = $"Producto {i}", Descripcion = "Referencia para catálogo",
                        Precio = 100, Stock = 10, IdCategoria = category, CategoriaNombre = categories[category - 1].Nombre
                    }).ToList()
                };
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(data) });
        }
    }

    private sealed class UsuarioAdmin : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, siguiente) =>
            {
                if (context.Request.Headers["X-Test-Admin"] == "true")
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.Name, "Admin de prueba"), new Claim(ClaimTypes.Role, "Admin")], "Test"));
                await siguiente();
            });
            next(app);
        };
    }
}
