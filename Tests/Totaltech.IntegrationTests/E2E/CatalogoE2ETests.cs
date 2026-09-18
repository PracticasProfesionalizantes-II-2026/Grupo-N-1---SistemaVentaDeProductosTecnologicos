using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

[Collection("Catalogo E2E")]
[Trait("Category", "E2E")]
public sealed class CatalogoE2ETests(CatalogoE2EFixture fixture)
{
    [Fact]
    public async Task Carrito_DetalleAgregarModificarEliminarYEstadoVacio()
    {
        var (email, contrasena) = await fixture.CrearClienteAsync();
        var page = await fixture.Browser.NewPageAsync(new() { ViewportSize = new() { Width = 375, Height = 812 } });
        await page.GotoAsync($"{fixture.FrontendUrl}/Home/Login");
        await page.GetByLabel("Email").FillAsync(email);
        await page.GetByLabel("Contraseña").FillAsync(contrasena);
        await page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos");
        await page.Locator(".catalogo__grilla article").First.GetByRole(AriaRole.Link, new() { Name = "Ver producto" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Agregar al carrito" }).ClickAsync();
        Assert.True((await page.ContentAsync()).Contains("Mi carrito", StringComparison.Ordinal),
            $"URL: {page.Url}; contenido: {await page.Locator("body").InnerTextAsync()}");
        await Assertions.Expect(page.Locator(".cart-badge")).ToHaveTextAsync("1");
        await page.Locator("input[name=cantidad]").FillAsync("2");
        await page.GetByRole(AriaRole.Button, new() { Name = "Actualizar" }).ClickAsync();
        await Assertions.Expect(page.Locator(".cart-badge")).ToHaveTextAsync("2");
        await page.GetByRole(AriaRole.Button, new() { Name = "Eliminar" }).ClickAsync();
        await Assertions.Expect(page.GetByText("Tu carrito está vacío.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Catalogo_EsUsableEnLosSeisAnchosYConTeclado()
    {
        var page = await fixture.Browser.NewPageAsync();
        foreach (var width in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(width, 800);
            await page.GotoAsync($"{fixture.FrontendUrl}/Productos");
            Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth"));
            await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Aplicar" })).ToBeVisibleAsync();
        }
        await page.Locator("#texto").FocusAsync();
        await page.Keyboard.PressAsync("Tab");
        Assert.False(await page.EvaluateAsync<bool>("document.activeElement === document.body"));
    }

    [Fact]
    public async Task CatalogoYDetalle_TresCargasQuedanBajoTresSegundos()
    {
        var page = await fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos");
        var detalle = await page.Locator(".catalogo__grilla article").First.GetByRole(AriaRole.Link, new() { Name = "Ver producto" }).GetAttributeAsync("href");
        foreach (var url in new[] { "/Productos", detalle!, "/Productos", detalle!, "/Productos", detalle! })
        {
            var inicio = DateTime.UtcNow;
            await page.GotoAsync(fixture.FrontendUrl + url, new() { WaitUntil = WaitUntilState.NetworkIdle });
            Assert.True(DateTime.UtcNow - inicio < TimeSpan.FromSeconds(3), $"La carga de {url} superó 3 segundos.");
        }
    }

    [Fact]
    public async Task Catalogo_BuscaFiltraPaginaYAbreDetalle()
    {
        var page = await fixture.Browser.NewPageAsync(new() { ViewportSize = new() { Width = 375, Height = 812 } });
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos");
        await Assertions.Expect(page.Locator(".catalogo__grilla article")).ToHaveCountAsync(12);
        await page.Locator("#texto").FillAsync("notebook");
        await page.GetByLabel("Sólo disponibles").CheckAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Aplicar" }).ClickAsync();
        await Assertions.Expect(page.GetByText("resultado(s)")).Not.ToHaveTextAsync("0 resultado(s)");
        await page.Locator(".catalogo__grilla article").First.GetByRole(AriaRole.Link, new() { Name = "Ver producto" }).ClickAsync();
        await Assertions.Expect(page.Locator("h1")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("Volver al catálogo")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Catalogo_CeroResultadosSePuedeLimpiarYProductoInexistenteEs404()
    {
        var page = await fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos?texto=no-existe-xyz");
        await Assertions.Expect(page.GetByText("No hay productos que coincidan")).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "Limpiar filtros" }).ClickAsync();
        await Assertions.Expect(page.Locator(".catalogo__grilla")).ToBeVisibleAsync();
        var response = await page.GotoAsync($"{fixture.FrontendUrl}/Productos/Detalle/2147483647");
        Assert.Equal(404, response?.Status);
    }
}
