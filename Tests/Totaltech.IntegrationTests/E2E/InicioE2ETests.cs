using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

[Collection("Catalogo E2E")]
[Trait("Category", "E2E")]
public sealed class InicioE2ETests(CatalogoE2EFixture fixture)
{
    [Fact]
    public async Task InicioMuestraCatalogoCompletoDestacadosYEsResponsive()
    {
        var producto = await fixture.CrearProductoAsync("Producto de portada " + Guid.NewGuid().ToString("N"));
        await using var context = await fixture.Browser.NewContextAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(fixture.FrontendUrl + "/Productos");
        var total = int.Parse((await page.Locator(".catalogo__encabezado p").InnerTextAsync()).Split(' ')[0]);
        await page.GotoAsync(fixture.FrontendUrl + "/");
        await Assertions.Expect(page.Locator("[data-inicio-productos] article")).ToHaveCountAsync(total);
        await Assertions.Expect(page.Locator(".inicio__destacados article")).ToHaveCountAsync(6);
        Assert.True(await page.Locator("[data-inicio-categorias] > a").CountAsync() > 0);
        Assert.True(await page.Locator("[data-inicio-categorias] > a").EvaluateAllAsync<bool>(
            "links => links.every(a => new URL(a.href).searchParams.get('idCategoria') === a.dataset.categoriaId)"));
        await Assertions.Expect(page.Locator(".inicio__destacados .inicio-producto__disponibilidad--agotado")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Iniciar sesión", Exact = true })).ToBeVisibleAsync();
        var tarjeta = page.Locator("[data-inicio-productos] article[data-producto-id='" + producto.IdProducto + "']");
        await Assertions.Expect(tarjeta.GetByText("Imagen no disponible")).ToBeVisibleAsync();
        foreach (var ancho in new[] { 320, 375, 425, 576, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(ancho, 900);
            Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth"), "Overflow en " + ancho);
            var columnas = ancho >= 1280 ? 4 : ancho >= 1024 ? 3 : ancho >= 768 ? 2 : 1;
            Assert.Equal(columnas, await page.Locator("[data-inicio-productos]").EvaluateAsync<int>("el => getComputedStyle(el).gridTemplateColumns.split(' ').length"));
            var categorias = ancho >= 1280 ? 4 : ancho >= 1024 ? 3 : ancho >= 576 ? 2 : 1;
            Assert.Equal(categorias, await page.Locator("[data-inicio-categorias]").EvaluateAsync<int>("el => getComputedStyle(el).gridTemplateColumns.split(' ').length"));
        }
        await page.GetByRole(AriaRole.Link, new() { Name = "Explorar productos" }).FocusAsync();
        await page.Keyboard.PressAsync("Tab");
        Assert.False(await page.EvaluateAsync<bool>("document.activeElement === document.body"));
        Assert.Equal("0s", await tarjeta.EvaluateAsync<string>("el => getComputedStyle(el).transitionDuration"));
        await CapturarAsync(page, "inicio-escritorio.png");
        await page.SetViewportSizeAsync(375, 812);
        await CapturarAsync(page, "inicio-movil.png");
        await tarjeta.GetByRole(AriaRole.Link, new() { Name = "Ver producto" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = producto.Nombre })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ClienteCierraSesionYConservaCarritoYPedidosDemo()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var cliente = await fixture.CrearClienteAsync();
        var producto = await fixture.CrearProductoAsync("Producto logout " + Guid.NewGuid().ToString("N"));
        await LoginAsync(page, cliente.Email, cliente.Contrasena);
        await page.GotoAsync(fixture.FrontendUrl + "/Productos/Detalle/" + producto.IdProducto);
        await page.GetByRole(AriaRole.Button, new() { Name = "Agregar al carrito" }).ClickAsync();
        var antes = await fixture.ObtenerEstadoCompraAsync(cliente.Email, producto.IdProducto);
        await page.GetByRole(AriaRole.Link, new() { Name = "Simular compra", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "¡Compra ficticia aprobada!" })).ToBeVisibleAsync();
        var historial = await page.EvaluateAsync<string>("JSON.stringify(localStorage)");
        await page.GotoAsync(fixture.FrontendUrl + "/");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Iniciar sesión", Exact = true })).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator(".header-actions .header-logout button")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".main-navigation button")).ToHaveCountAsync(0);
        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Iniciar sesión", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true })).ToHaveCountAsync(0);
        Assert.Equal(historial, await page.EvaluateAsync<string>("JSON.stringify(localStorage)"));
        Assert.Equal(antes, await fixture.ObtenerEstadoCompraAsync(cliente.Email, producto.IdProducto));
        await page.GotoAsync(fixture.FrontendUrl + "/Carrito");
        Assert.Contains("/Home/Login", page.Url);
        await LoginAsync(page, cliente.Email, cliente.Contrasena);
        await page.GotoAsync(fixture.FrontendUrl + "/Carrito");
        await Assertions.Expect(page.Locator("input[name=cantidad]")).ToHaveValueAsync("1");
    }

    [Fact]
    public async Task AdministradorPuedeCerrarSesionDesdeTiendaSinCambiarPanel()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var administrador = await fixture.CrearAdministradorAsync();
        await LoginAsync(page, administrador.Email, administrador.Contrasena);
        Assert.Contains("/Administracion", page.Url);
        await page.GotoAsync(fixture.FrontendUrl + "/");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Administración", Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Iniciar sesión", Exact = true })).ToBeVisibleAsync();
    }

    private async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync(fixture.FrontendUrl + "/Home/Login");
        await page.GetByLabel("Email").FillAsync(email);
        await page.GetByLabel("Contraseña").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión", Exact = true }).ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Home/Login"));
    }

    private static async Task CapturarAsync(IPage page, string nombre)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(dir);
        await page.ScreenshotAsync(new() { Path = Path.Combine(dir, nombre), FullPage = true, Animations = ScreenshotAnimations.Disabled });
    }
}
