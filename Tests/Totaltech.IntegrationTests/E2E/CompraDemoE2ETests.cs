using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

[Collection("Catalogo E2E")]
[Trait("Category", "E2E")]
public sealed class CompraDemoE2ETests(CatalogoE2EFixture fixture)
{
    [Fact]
    public async Task AprobacionPersisteSeguimientoYSnapshotSinModificarDatosReales()
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 375, Height = 812 } });
        var datos = await PrepararAsync(context);
        var page = datos.Page;
        var antes = await fixture.ObtenerEstadoCompraAsync(datos.Email, datos.IdProducto);
        var escrituras = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method is not ("GET" or "HEAD")) escrituras.Add(request.Url);
        };
        await page.GetByRole(AriaRole.Link, new() { Name = "Simular compra", Exact = true }).ClickAsync();
        await CapturarAsync(page, "compra-demo-checkout.png");
        await AprobarAsync(page);
        var id = await page.Locator("[data-demo-resumen] h2").InnerTextAsync();
        await page.ReloadAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "¡Compra ficticia aprobada!" })).ToBeVisibleAsync();
        await CapturarAsync(page, "compra-demo-confirmacion.png");
        await page.GetByRole(AriaRole.Link, new() { Name = "Ver seguimiento demo" }).ClickAsync();
        foreach (var estado in new[] { "En preparación", "Enviado", "Entregado" })
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Avanzar estado demo" }).ClickAsync();
            await Assertions.Expect(page.Locator("[data-demo-timeline] [aria-current]")).ToHaveTextAsync(estado);
        }
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Avanzar estado demo" })).ToBeDisabledAsync();
        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("[data-demo-timeline] [aria-current]")).ToHaveTextAsync("Entregado");
        await page.SetViewportSizeAsync(1280, 900);
        await CapturarAsync(page, "compra-demo-seguimiento.png");
        await page.GotoAsync(fixture.FrontendUrl + "/Pedidos");
        await Assertions.Expect(page.Locator(".compra-demo__pedidos article")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByText(id, new() { Exact = true })).ToBeVisibleAsync();
        Assert.Equal(antes, await fixture.ObtenerEstadoCompraAsync(datos.Email, datos.IdProducto));
        Assert.Empty(escrituras);

        // Una edición posterior del carrito no altera la copia del pedido aprobado.
        await page.GotoAsync(fixture.FrontendUrl + "/Carrito");
        await page.Locator("input[name=cantidad]").FillAsync("2");
        await page.GetByRole(AriaRole.Button, new() { Name = "Actualizar", Exact = true }).ClickAsync();
        await page.GotoAsync(fixture.FrontendUrl + "/Pedidos/Detalle/" + id);
        await Assertions.Expect(page.Locator(".compra-demo__total")).ToContainTextAsync("250,00");
        await page.GetByRole(AriaRole.Link, new() { Name = "Mis pedidos demo", Exact = true }).Last.ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Borrar mis pedidos demo" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Conservar pedidos" }).ClickAsync();
        await Assertions.Expect(page.Locator(".compra-demo__pedidos article")).ToHaveCountAsync(1);
        await page.GetByRole(AriaRole.Button, new() { Name = "Borrar mis pedidos demo" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar borrado demo" }).ClickAsync();
        await Assertions.Expect(page.Locator(".compra-demo__pedidos article")).ToHaveCountAsync(0);
        await page.GotoAsync(fixture.FrontendUrl + "/Carrito");
        await Assertions.Expect(page.Locator("input[name=cantidad]")).ToHaveValueAsync("2");
    }

    [Fact]
    public async Task RechazoDemoraRecargaYDobleEnvioNoDuplicanPedidos()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var datos = await PrepararAsync(context);
        var page = datos.Page;
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout?nuevo=true");
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" })).ToBeEnabledAsync();
        await page.Locator("#demo-escenario").SelectOptionAsync("rechazo");
        await page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" })).ToBeDisabledAsync();
        await Assertions.Expect(page.Locator("[data-demo-feedback]")).ToContainTextAsync("rechazado");
        Assert.Equal(0, await CantidadPedidosAsync(page));
        await page.Locator("#demo-escenario").SelectOptionAsync("demora");
        await page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" }).ClickAsync();
        await Assertions.Expect(page.Locator("[data-demo-feedback]")).ToContainTextAsync("agotó el tiempo", new() { Timeout = 10000 });
        Assert.Equal(0, await CantidadPedidosAsync(page));
        await page.Locator("#demo-escenario").SelectOptionAsync("aprobacion");
        await page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" }).ClickAsync();
        var intento = await page.EvaluateAsync<string>("() => Object.keys(sessionStorage).find(k => k.endsWith('.intento')) && JSON.parse(sessionStorage.getItem(Object.keys(sessionStorage).find(k => k.endsWith('.intento')))).id");
        await page.ReloadAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" })).ToBeEnabledAsync();
        Assert.Equal(0, await CantidadPedidosAsync(page));
        await page.EvaluateAsync("() => { const f = document.querySelector('[data-demo-form]'); f.requestSubmit(); f.requestSubmit(); }");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "¡Compra ficticia aprobada!" })).ToBeVisibleAsync();
        Assert.Equal(1, await CantidadPedidosAsync(page));
        await Assertions.Expect(page.Locator("[data-demo-resumen] h2")).ToHaveTextAsync(intento);
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "¡Compra ficticia aprobada!" })).ToBeVisibleAsync();
        Assert.Equal(1, await CantidadPedidosAsync(page));
    }

    [Theory]
    [InlineData("lectura")]
    [InlineData("escritura")]
    [InlineData("sesion")]
    public async Task AlmacenamientoBloqueadoNoDeclaraCompraCompletada(string modo)
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var datos = await PrepararAsync(context);
        var page = datos.Page;
        await page.AddInitScriptAsync(modo switch
        {
            "lectura" => "Object.defineProperty(window, 'localStorage', { get() { throw new DOMException('Bloqueado'); } });",
            "escritura" => "const original = Storage.prototype.setItem; Storage.prototype.setItem = function(k,v) { if (this === localStorage) throw new DOMException('Sin espacio'); return original.call(this,k,v); };",
            _ => "Object.defineProperty(window, 'sessionStorage', { get() { throw new DOMException('Bloqueado'); } });"
        });
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout");
        if (modo == "escritura") await page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" }).ClickAsync();
        await Assertions.Expect(page.Locator("[data-demo-feedback]")).ToContainTextAsync("almacenamiento");
        Assert.DoesNotContain("/Confirmacion", page.Url);
        await page.GotoAsync(fixture.FrontendUrl + "/Carrito");
        await Assertions.Expect(page.Locator("input[name=cantidad]")).ToHaveValueAsync("1");
    }

    [Fact]
    public async Task HistorialPorUsuarioCorrupcionYContenidoSeRenderizanDeFormaSegura()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var datos = await PrepararAsync(context);
        var page = datos.Page;
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout");
        await AprobarAsync(page);
        var id = await page.Locator("[data-demo-resumen] h2").InnerTextAsync();
        await page.EvaluateAsync(@"() => {
            const key = Object.keys(localStorage).find(k => k.startsWith('Totaltech.CompraDemo'));
            const d = JSON.parse(localStorage.getItem(key));
            d.pedidos[0].carrito.items[0].nombre = '<img src=x onerror=window.demoXss=true>';
            localStorage.setItem(key, JSON.stringify(d));
        }");
        await page.GotoAsync(fixture.FrontendUrl + "/Pedidos/Detalle/" + id);
        await Assertions.Expect(page.GetByText("<img src=x onerror=window.demoXss=true>", new() { Exact = true })).ToBeVisibleAsync();
        Assert.False(await page.EvaluateAsync<bool>("() => window.demoXss === true"));
        await Assertions.Expect(page.Locator("[data-demo-resumen] img")).ToHaveCountAsync(0);
        var segundo = await fixture.CrearClienteAsync();
        await LoginAsync(page, segundo.Email, segundo.Contrasena);
        await page.GotoAsync(fixture.FrontendUrl + "/Pedidos");
        await Assertions.Expect(page.Locator(".compra-demo__pedidos article")).ToHaveCountAsync(0);
        await page.GotoAsync(fixture.FrontendUrl + "/Pedidos/Detalle/" + id);
        await Assertions.Expect(page.Locator("[data-demo-feedback]")).ToContainTextAsync("No se encontró");
        await LoginAsync(page, datos.Email, datos.Password);
        await page.GotoAsync(fixture.FrontendUrl + "/Pedidos");
        await Assertions.Expect(page.Locator(".compra-demo__pedidos article")).ToHaveCountAsync(1);
        await page.EvaluateAsync(@"() => {
            const key = Object.keys(localStorage).find(k => k.startsWith('Totaltech.CompraDemo'));
            localStorage.setItem(key, 'formato roto');
            localStorage.setItem('otro-modulo', 'conservar');
        }");
        await page.ReloadAsync();
        await Assertions.Expect(page.Locator("[data-demo-feedback]")).ToContainTextAsync("formato inválido");
        await page.GetByRole(AriaRole.Button, new() { Name = "Borrar mis pedidos demo" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Confirmar borrado demo" }).ClickAsync();
        await Assertions.Expect(page.Locator(".compra-demo__pedidos article")).ToHaveCountAsync(0);
        Assert.Equal("conservar", await page.EvaluateAsync<string>("() => localStorage.getItem('otro-modulo')"));
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout/Confirmacion?id=DEMO-no-existe");
        await Assertions.Expect(page.Locator("[data-demo-feedback]")).ToContainTextAsync("No se encontró");
        await Assertions.Expect(page.Locator(".compra-demo__check")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task FlagApagadoOcultaAccesosYBloqueaRutasSinRomperCarrito()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var datos = await PrepararAsync(context);
        try
        {
            await fixture.ReiniciarFrontendAsync(compraDemo: false);
            await LoginAsync(datos.Page, datos.Email, datos.Password);
            await datos.Page.GotoAsync(fixture.FrontendUrl + "/Carrito");
            await Assertions.Expect(datos.Page.GetByRole(AriaRole.Link, new() { Name = "Simular compra", Exact = true })).ToHaveCountAsync(0);
            await Assertions.Expect(datos.Page.GetByRole(AriaRole.Link, new() { Name = "Mis pedidos demo", Exact = true })).ToHaveCountAsync(0);
            await Assertions.Expect(datos.Page.Locator("input[name=cantidad]")).ToHaveValueAsync("1");
            foreach (var ruta in new[] { "/Checkout", "/Checkout/Confirmacion?id=DEMO-x", "/Pedidos", "/Pedidos/Detalle/DEMO-x" })
                Assert.Equal(404, (await datos.Page.GotoAsync(fixture.FrontendUrl + ruta))?.Status);
            await context.ClearCookiesAsync();
            foreach (var ruta in new[] { "/Checkout", "/Checkout/Confirmacion", "/Pedidos", "/Pedidos/Detalle/DEMO-x" })
                Assert.Equal(404, (await datos.Page.GotoAsync(fixture.FrontendUrl + ruta))?.Status);
        }
        finally { await fixture.ReiniciarFrontendAsync(); }
    }

    [Fact]
    public async Task DemoEsResponsiveAccesibleYCarritoVacioNoSimula()
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        var datos = await PrepararAsync(context);
        var page = datos.Page;
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout");
        foreach (var ancho in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(ancho, 900);
            Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth"), "Overflow en " + ancho);
            await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" })).ToBeVisibleAsync();
        }
        await page.Locator("#demo-escenario").FocusAsync();
        await page.Keyboard.PressAsync("Tab");
        Assert.Equal("Simular pago", await page.EvaluateAsync<string>("() => document.activeElement.textContent"));
        Assert.Equal("none", await page.Locator(".compra-demo__panel").First.EvaluateAsync<string>("el => getComputedStyle(el).animationName"));
        await AprobarAsync(page);
        foreach (var ruta in new[] { page.Url, fixture.FrontendUrl + "/Pedidos", fixture.FrontendUrl + "/Pedidos/Detalle/" + await ObtenerIdAsync(page) })
        {
            await page.GotoAsync(ruta);
            foreach (var ancho in new[] { 320, 375, 425, 768, 1024, 1280 })
            {
                await page.SetViewportSizeAsync(ancho, 900);
                Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth"), ruta + " overflow en " + ancho);
            }
        }
        await page.GotoAsync(fixture.FrontendUrl + "/Carrito");
        await page.GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true }).ClickAsync();
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Tu carrito está vacío" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" })).ToHaveCountAsync(0);
        await context.ClearCookiesAsync();
        await page.GotoAsync(fixture.FrontendUrl + "/Checkout");
        Assert.Contains("/Home/Login", page.Url);
    }

    private async Task<(IPage Page, string Email, string Password, int IdProducto)> PrepararAsync(IBrowserContext context)
    {
        var cliente = await fixture.CrearClienteAsync();
        var producto = await fixture.CrearProductoAsync("Producto demo " + Guid.NewGuid().ToString("N"));
        var page = await context.NewPageAsync();
        await LoginAsync(page, cliente.Email, cliente.Contrasena);
        await page.GotoAsync(fixture.FrontendUrl + "/Productos/Detalle/" + producto.IdProducto);
        await page.GetByRole(AriaRole.Button, new() { Name = "Agregar al carrito", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Mi carrito" })).ToBeVisibleAsync();
        return (page, cliente.Email, cliente.Contrasena, producto.IdProducto);
    }

    private async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync(fixture.FrontendUrl + "/Home/Login");
        await page.GetByLabel("Email").FillAsync(email);
        await page.GetByLabel("Contraseña").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión", Exact = true }).ClickAsync();
        await page.WaitForURLAsync(fixture.FrontendUrl + "/");
    }

    private static async Task AprobarAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Simular pago" }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "¡Compra ficticia aprobada!" })).ToBeVisibleAsync();
    }

    private static Task<int> CantidadPedidosAsync(IPage page) => page.EvaluateAsync<int>(
        "() => { const k = Object.keys(localStorage).find(k => k.startsWith('Totaltech.CompraDemo')); return k ? JSON.parse(localStorage.getItem(k)).pedidos.length : 0; }");

    private static Task<string> ObtenerIdAsync(IPage page) => page.Locator("[data-demo-resumen] h2").InnerTextAsync();

    private static async Task CapturarAsync(IPage page, string nombre)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(dir);
        await page.ScreenshotAsync(new() { Path = Path.Combine(dir, nombre), FullPage = true, Animations = ScreenshotAnimations.Disabled });
    }
}
