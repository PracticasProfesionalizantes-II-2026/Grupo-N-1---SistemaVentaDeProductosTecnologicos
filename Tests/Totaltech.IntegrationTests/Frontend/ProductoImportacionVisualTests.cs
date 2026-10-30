using System.Text;
using Microsoft.Playwright;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ProductoImportacionVisualTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RevisionResponsiveConTecladoInvalidacionDeCambiosYConfirmacion()
    {
        using var api = new ImportacionApiPrueba();
        await using var factory = ImportacionHttp.Factory(api);
        using var http = ImportacionHttp.Cliente(factory);
        await ImportacionHttp.LoginAsync(http);
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        var erroresScript = new List<string>();
        page.PageError += (_, error) => erroresScript.Add(error);
        // El navegador usa el host MVC real de TestServer y su cookie, sin abrir puertos ni usar SQL.
        await page.RouteAsync("https://frontend.test/**", async route =>
        {
            using var request = new HttpRequestMessage(new HttpMethod(route.Request.Method), route.Request.Url);
            if (route.Request.PostDataBuffer is { } body)
            {
                request.Content = new ByteArrayContent(body);
                if (route.Request.Headers.TryGetValue("content-type", out var contentType))
                    request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            }
            using var initial = await http.SendAsync(request);
            // Resolver PRG dentro del puente: frontend.test sólo existe en TestServer.
            using var response = initial.Headers.Location is { } location
                ? await http.GetAsync(new Uri(http.BaseAddress!, location))
                : initial;
            var headers = response.Headers.Concat(response.Content.Headers)
                .Where(h => !h.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(h => h.Key, h => string.Join(",", h.Value));
            await route.FulfillAsync(new() { Status = (int)response.StatusCode, Headers = headers,
                BodyBytes = await response.Content.ReadAsByteArrayAsync() });
        });
        await page.GotoAsync("https://frontend.test/Productos/Importar");
        await page.GetByLabel("Archivo del catálogo").SetInputFilesAsync(new FilePayload
        {
            Name = "catalogo.csv", MimeType = "text/csv",
            Buffer = Encoding.UTF8.GetBytes("Nombre;Descripcion;Precio;Stock;CategoriaNombre\nNotebook de prueba;Equipo para revisar;1234,56;5;Notebooks")
        });
        await page.Locator("#upload-proveedor").SelectOptionAsync("1");
        await page.GetByRole(AriaRole.Button, new() { Name = "Extraer y previsualizar" }).ClickAsync();
        var confirmar = page.GetByRole(AriaRole.Button, new() { Name = "Confirmar importación completa" });
        await Expect(confirmar).ToBeEnabledAsync();
        var directorio = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestResults", "UC28"));
        Directory.CreateDirectory(directorio);
        foreach (var ancho in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(ancho, 900);
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), $"Overflow horizontal a {ancho}px");
            Assert.True(await page.EvaluateAsync<bool>(@"() => [...document.querySelectorAll('[data-importacion-revision] input:not([type=hidden]), [data-importacion-revision] select, textarea')].every(e => [...e.labels].length > 0)"));
            await page.ScreenshotAsync(new() { Path = Path.Combine(directorio, $"importacion-{ancho}.png"), FullPage = true,
                Animations = ScreenshotAnimations.Disabled });
        }
        var nombre = page.Locator("[name='Filas[0].Nombre']");
        await nombre.FocusAsync();
        await page.Keyboard.PressAsync("End");
        await page.Keyboard.TypeAsync(" revisada");
        await Expect(confirmar).ToBeDisabledAsync();
        await Expect(page.Locator("[data-importacion-estado]")).ToContainTextAsync("cambios pendientes");
        await page.GetByRole(AriaRole.Button, new() { Name = "Validar cambios" }).ClickAsync();
        await Expect(confirmar).ToBeEnabledAsync();
        Assert.Equal(0, api.Confirmaciones);
        await confirmar.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Se importaron 1 productos correctamente." })).ToBeVisibleAsync();
        Assert.Equal(1, api.Confirmaciones);
        Assert.Empty(erroresScript);
        output.WriteLine("Capturas responsive verificadas: " + directorio);
    }
}
