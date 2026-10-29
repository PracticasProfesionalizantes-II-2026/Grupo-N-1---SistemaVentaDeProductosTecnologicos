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
    public async Task AdministradorPuedeCerrarSesionDesdeTiendaYVolverALogin()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var administrador = await fixture.CrearAdministradorAsync();
        await LoginAsync(page, administrador.Email, administrador.Contrasena);
        Assert.Contains("/Administracion", page.Url);
        await page.GotoAsync(fixture.FrontendUrl + "/");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Administración", Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Iniciar sesión", Exact = true })).ToBeVisibleAsync();
        Assert.Equal(fixture.FrontendUrl + "/Home/Login", page.Url);
    }

    [Fact]
    public async Task AdministradorRecibeBienvenidaUnaVezYCierraSesionDesdePanel()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var administrador = await fixture.CrearAdministradorAsync();
        await LoginAsync(page, administrador.Email, administrador.Contrasena);
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(1);
        await CapturarAsync(page, "admin-bienvenida.png");
        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(0);
        await page.GotoAsync(fixture.FrontendUrl + "/Home/Login");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Iniciar sesión", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(0);
        await page.GotoAsync(fixture.FrontendUrl + "/Administracion");

        foreach (var ancho in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(ancho, 900);
            await page.ReloadAsync();
            if (ancho <= 768)
                await page.GetByRole(AriaRole.Button, new() { Name = "Abrir menú de administración", Exact = true }).ClickAsync();
            var cerrarSesion = page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true });
            await Assertions.Expect(cerrarSesion).ToBeVisibleAsync();
            await cerrarSesion.FocusAsync();
            await Assertions.Expect(cerrarSesion).ToBeFocusedAsync();
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
            Assert.DoesNotContain("Modo Administrador Activo", await page.ContentAsync());
            if (ancho is 320 or 1280) await CapturarAsync(page, $"admin-sesion-{ancho}.png");
        }

        await page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true }).PressAsync("Enter");
        await Assertions.Expect(page).ToHaveURLAsync(fixture.FrontendUrl + "/Home/Login");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Iniciar sesión", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true })).ToHaveCountAsync(0);
        await page.GotoAsync(fixture.FrontendUrl + "/Administracion");
        Assert.Contains("/Home/Login", page.Url);
    }

    [Theory]
    [InlineData("Usuario")]
    [InlineData("María Alejandra de los Ángeles Fernández de la Cruz")]
    public async Task BienvenidaCompactaJuntoAlCarritoEsResponsiveYApareceUnaVez(string nombre)
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        var cliente = await fixture.CrearClienteAsync(nombre);
        await LoginAsync(page, cliente.Email, cliente.Contrasena);
        var bienvenida = page.Locator(".header-actions [data-auth-welcome]");
        await Assertions.Expect(bienvenida).ToHaveTextAsync($"¡Bienvenido, {nombre}!");
        await Assertions.Expect(bienvenida).ToHaveAttributeAsync("title", $"¡Bienvenido, {nombre}!");
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("main [data-auth-welcome]")).ToHaveCountAsync(0);
        await Assertions.Expect(bienvenida.GetByRole(AriaRole.Img)).ToHaveCountAsync(0);

        foreach (var ancho in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(ancho, 900);
            await Assertions.Expect(bienvenida).ToBeVisibleAsync();
            var carrito = page.Locator(".header-actions .cart-link");
            var cerrarSesion = page.GetByRole(AriaRole.Button, new() { Name = "Cerrar sesión", Exact = true });
            await Assertions.Expect(carrito).ToBeVisibleAsync();
            await Assertions.Expect(cerrarSesion).ToBeVisibleAsync();
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), $"Overflow a {ancho}px");
            var saludoRect = (await bienvenida.BoundingBoxAsync())!;
            var carritoRect = (await carrito.BoundingBoxAsync())!;
            var logoutRect = (await cerrarSesion.BoundingBoxAsync())!;
            if (ancho is 320 or 375 or 1280)
                await CapturarAsync(page, $"bienvenida-{(nombre == "Usuario" ? "corta" : "larga")}-{ancho}.png", paginaCompleta: false);
            if (ancho <= 991)
            {
                Assert.True(saludoRect.Y + saludoRect.Height <= carritoRect.Y, $"El saludo debe estar encima del carrito a {ancho}px");
                Assert.InRange(Math.Abs(saludoRect.X + saludoRect.Width - logoutRect.X - logoutRect.Width), 0, 1);
            }
            else
            {
                Assert.True(saludoRect.X + saludoRect.Width <= carritoRect.X, $"El saludo debe preceder al carrito a {ancho}px");
            }
            Assert.True(await bienvenida.Locator(".header-welcome__text").EvaluateAsync<bool>(
                "el => getComputedStyle(el).whiteSpace === 'nowrap' && getComputedStyle(el).textOverflow === 'ellipsis'"));
            await cerrarSesion.FocusAsync();
            await Assertions.Expect(cerrarSesion).ToBeFocusedAsync();
        }

        await page.GotoAsync(fixture.FrontendUrl + "/Productos");
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(0);
        await LoginAsync(page, cliente.Email, cliente.Contrasena);
        await Assertions.Expect(bienvenida).ToBeVisibleAsync();
        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(0);
        await page.GotoAsync(fixture.FrontendUrl + "/Home/Login");
        await Assertions.Expect(page.Locator("[data-auth-welcome]")).ToHaveCountAsync(0);
    }

    private async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync(fixture.FrontendUrl + "/Home/Login");
        await page.GetByLabel("Email").FillAsync(email);
        await page.GetByLabel("Contraseña").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión", Exact = true }).ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/Home/Login"));
    }

    private static async Task CapturarAsync(IPage page, string nombre, bool paginaCompleta = true)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(dir);
        await page.ScreenshotAsync(new() { Path = Path.Combine(dir, nombre), FullPage = paginaCompleta, Animations = ScreenshotAnimations.Disabled });
    }
}
