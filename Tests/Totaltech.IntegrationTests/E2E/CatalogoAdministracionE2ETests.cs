using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

[Collection("Catalogo E2E")]
[Trait("Category", "E2E")]
public sealed class CatalogoAdministracionE2ETests(CatalogoE2EFixture fixture)
{
    private const string NombreConImagen = "Notebook Lenovo Yoga 7 2 en 1";
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aU1sAAAAASUVORK5CYII=");

    [Fact]
    public async Task Administracion_CatalogoYEdicionSonUsablesEnSeisAnchosYConTeclado()
    {
        var clave = $"Responsive-{Guid.NewGuid():N}";
        await fixture.CrearProductoAsync(NombreConImagen, clave);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);

        foreach (var width in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(width, 900);
            await page.GotoAsync(Catalogo(clave));
            await SinDesbordeHorizontalAsync(page);
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Nuevo producto" })).ToBeVisibleAsync();
            var tarjeta = page.Locator(".catalogo__grilla article");
            await Assertions.Expect(tarjeta.GetByRole(AriaRole.Link, new() { Name = "Ver producto" })).ToBeVisibleAsync();
            await Assertions.Expect(tarjeta.GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true })).ToBeVisibleAsync();
            var editar = tarjeta.GetByRole(AriaRole.Link, new() { Name = "Editar", Exact = true });
            if (width is 320 or 1280) await CapturarAsync(page, $"catalogo-{width}.png");
            await editar.FocusAsync();
            await Assertions.Expect(editar).ToBeFocusedAsync();
            await page.Keyboard.PressAsync("Enter");
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Editar producto" })).ToBeVisibleAsync();
            await SinDesbordeHorizontalAsync(page);
            await Assertions.Expect(page.GetByLabel("Imagen de referencia", new() { Exact = true })).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" })).ToBeVisibleAsync();
            if (width is 320 or 1280) await CapturarAsync(page, $"editar-{width}.png");
            var cancelar = page.GetByRole(AriaRole.Link, new() { Name = "Cancelar", Exact = true });
            await cancelar.FocusAsync();
            await Assertions.Expect(cancelar).ToBeFocusedAsync();
            await page.Keyboard.PressAsync("Enter");
            await Assertions.Expect(page).ToHaveURLAsync(Catalogo(clave));
        }
    }

    [Fact]
    public async Task Editar_GuardaOCancelaConservandoFiltrosYPagina()
    {
        var clave = $"Editar-{Guid.NewGuid():N}";
        var productos = await CrearPaginaCompletaYUnaTarjetaAsync(clave);
        var catalogo = CatalogoFiltrado(clave, productos[0].IdCategoria, pagina: 2);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        await page.GotoAsync(catalogo);
        await Assertions.Expect(page.Locator(".catalogo__grilla article")).ToHaveCountAsync(1);
        var nombre = await page.Locator(".catalogo__grilla article h2").InnerTextAsync();
        var producto = productos.Single(item => item.Nombre == nombre);
        await AbrirEdicionAsync(page);
        await Assertions.Expect(page.Locator("form[enctype='multipart/form-data']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByLabel("Nombre", new() { Exact = true })).ToHaveValueAsync(nombre);
        await page.GetByLabel("Nombre", new() { Exact = true }).FillAsync($"{nombre} cancelado");
        await page.GetByRole(AriaRole.Link, new() { Name = "Cancelar", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(catalogo);
        Assert.Equal(nombre, (await fixture.ObtenerProductoAsync(producto.IdProducto))?.Nombre);

        await AbrirEdicionAsync(page);
        await page.GetByLabel("Nombre", new() { Exact = true }).FillAsync($"{nombre} editado");
        await page.GetByLabel("Stock", new() { Exact = true }).FillAsync("7");
        await GuardarAsync(page);
        await Assertions.Expect(page).ToHaveURLAsync(catalogo);
        await Assertions.Expect(page.GetByText("Producto actualizado correctamente.", new() { Exact = true })).ToBeVisibleAsync();
        var actualizado = await fixture.ObtenerProductoAsync(producto.IdProducto);
        Assert.Equal($"{nombre} editado", actualizado?.Nombre);
        Assert.Equal(7, actualizado?.Stock);
    }

    [Fact]
    public async Task Imagen_SePrevisualizaReemplazaYConservaTrasRenombrarYReiniciar()
    {
        var producto = await fixture.CrearProductoAsync($"Imagen-{Guid.NewGuid():N}");
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        await page.GotoAsync(Catalogo(producto.Nombre));
        await AbrirEdicionAsync(page);
        await SeleccionarPngAsync(page);
        await Assertions.Expect(page.Locator("#imagen-preview")).ToHaveAttributeAsync("src", new Regex("^blob:"));
        await Assertions.Expect(page.Locator("#imagen-preview")).ToBeVisibleAsync();
        await GuardarAsync(page);
        var primera = (await fixture.ObtenerProductoAsync(producto.IdProducto))?.ImagenUrl;
        Assert.Matches("^/uploads/productos/[a-f0-9]{32}\\.png$", primera ?? string.Empty);
        await Assertions.Expect(page.Locator(".catalogo__grilla article img")).ToHaveAttributeAsync("src", primera!);

        await AbrirEdicionAsync(page);
        await Assertions.Expect(page.Locator("#imagen-preview")).ToHaveAttributeAsync("src", primera!);
        await SeleccionarPngAsync(page);
        await GuardarAsync(page);
        var segunda = (await fixture.ObtenerProductoAsync(producto.IdProducto))?.ImagenUrl;
        Assert.NotEqual(primera, segunda);
        await AbrirEdicionAsync(page);
        await page.GetByLabel("Nombre", new() { Exact = true }).FillAsync($"{producto.Nombre} nuevo");
        await GuardarAsync(page);
        Assert.Equal(segunda, (await fixture.ObtenerProductoAsync(producto.IdProducto))?.ImagenUrl);

        await fixture.ReiniciarFrontendAsync();
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos/Detalle/{producto.IdProducto}");
        var imagen = page.Locator("img.product-detail-image");
        await Assertions.Expect(imagen).ToHaveAttributeAsync("src", segunda!);
        await Assertions.Expect(imagen).ToBeVisibleAsync();
        Assert.True(await imagen.EvaluateAsync<bool>("img => img.complete && img.naturalWidth > 0"));
        var response = await context.APIRequest.GetAsync(fixture.FrontendUrl + segunda);
        Assert.Equal(200, response.Status);
        Assert.Equal(Png, await response.BodyAsync());
        await page.GotoAsync(Catalogo(producto.Nombre));
        await Assertions.Expect(page.Locator(".catalogo__grilla article img")).ToHaveAttributeAsync("src", segunda!);
    }

    [Fact]
    public async Task Editar_SinArchivoConservaImagenOriginalAunqueCambieElNombre()
    {
        var clave = $"Original-{Guid.NewGuid():N}";
        var producto = await fixture.CrearProductoAsync(NombreConImagen, clave);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        await page.GotoAsync(Catalogo(clave));
        var original = await page.Locator(".catalogo__grilla article img").GetAttributeAsync("src");
        Assert.StartsWith("/images/categorias/", original);
        await AbrirEdicionAsync(page);
        await Assertions.Expect(page.Locator("#imagen-preview")).ToHaveAttributeAsync("src", original!);
        await page.GetByLabel("Nombre", new() { Exact = true }).FillAsync($"Producto renombrado {clave}");
        await GuardarAsync(page);
        await Assertions.Expect(page.Locator(".catalogo__grilla article img")).ToHaveAttributeAsync("src", original!);
        Assert.Equal(original, (await fixture.ObtenerProductoAsync(producto.IdProducto))?.ImagenUrl);
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos/Detalle/{producto.IdProducto}");
        await Assertions.Expect(page.Locator("img.product-detail-image")).ToHaveAttributeAsync("src", original!);
    }

    [Theory]
    [InlineData("vacio")]
    [InlineData("firma")]
    [InlineData("mime")]
    [InlineData("extension")]
    [InlineData("tamano")]
    public async Task Imagen_InvalidaPreservaLosDatosIngresadosYLaImagenAnterior(string caso)
    {
        var clave = $"Invalida-{Guid.NewGuid():N}";
        var producto = await fixture.CrearProductoAsync(NombreConImagen, clave);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        await page.GotoAsync(Catalogo(clave));
        var original = await page.Locator(".catalogo__grilla article img").GetAttributeAsync("src");
        await AbrirEdicionAsync(page);
        await page.GetByLabel("Nombre", new() { Exact = true }).FillAsync($"{clave} cambios pendientes");
        var buffer = caso switch
        {
            "vacio" => [],
            "firma" => Encoding.UTF8.GetBytes("Este archivo no es una imagen."),
            "tamano" => new byte[5 * 1024 * 1024 + 1],
            _ => Png
        };
        await page.GetByLabel("Imagen de referencia", new() { Exact = true }).SetInputFilesAsync(new FilePayload
        {
            Name = caso == "extension" ? "imagen.svg" : "imagen.png",
            MimeType = caso == "mime" ? "image/jpeg" : "image/png", Buffer = buffer
        });
        // Omitimos la validación cliente para demostrar que el servidor también rechaza el archivo.
        await page.Locator("form[enctype='multipart/form-data']").EvaluateAsync("form => form.submit()");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Editar producto" })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-valmsg-for='ImagenArchivo']")).Not.ToBeEmptyAsync();
        await Assertions.Expect(page.GetByLabel("Nombre", new() { Exact = true })).ToHaveValueAsync($"{clave} cambios pendientes");
        await Assertions.Expect(page.Locator("#imagen-preview")).ToHaveAttributeAsync("src", original!);
        Assert.Equal(NombreConImagen, (await fixture.ObtenerProductoAsync(producto.IdProducto))?.Nombre);
        Assert.Null((await fixture.ObtenerProductoAsync(producto.IdProducto))?.ImagenUrl);
    }

    [Fact]
    public async Task Eliminar_ConfirmaElNombrePermiteCancelarYRetrocedeSiLaUltimaPaginaQuedaVacia()
    {
        var clave = $"Eliminar-{Guid.NewGuid():N}";
        var productos = await CrearPaginaCompletaYUnaTarjetaAsync(clave);
        var catalogo = CatalogoFiltrado(clave, productos[0].IdCategoria, pagina: 2);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        await page.GotoAsync(catalogo);
        var nombre = await page.Locator(".catalogo__grilla article h2").InnerTextAsync();
        var producto = productos.Single(item => item.Nombre == nombre);
        Assert.Contains(nombre, await EliminarConConfirmacionAsync(page, confirmar: false));
        await Assertions.Expect(page).ToHaveURLAsync(catalogo);
        Assert.NotNull(await fixture.ObtenerProductoAsync(producto.IdProducto));
        Assert.Contains(nombre, await EliminarConConfirmacionAsync(page, confirmar: true));
        await Assertions.Expect(page.GetByText("Producto eliminado correctamente.", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".catalogo__grilla article")).ToHaveCountAsync(12);
        Assert.Null(await fixture.ObtenerProductoAsync(producto.IdProducto));
        Assert.Contains($"texto={clave}", page.Url);
        Assert.Contains($"idCategoria={producto.IdCategoria}", page.Url);
        Assert.Contains("precioMin=100", page.Url);
        Assert.Contains("precioMax=300", page.Url);
        var filtros = QueryHelpers.ParseQuery(new Uri(page.Url).Query);
        Assert.True(bool.Parse(filtros["soloDisponibles"].ToString()));
        Assert.Contains("tamanoPagina=12", page.Url);
        Assert.DoesNotContain("pagina=2", page.Url);
    }

    [Fact]
    public async Task Eliminar_ProductoAsociadoAUnCarritoSeConservaYMuestraElMotivo()
    {
        var producto = await fixture.CrearProductoAsync($"Relacionado-{Guid.NewGuid():N}");
        await fixture.AsociarCarritoAsync(producto.IdProducto);
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        await page.GotoAsync(Catalogo(producto.Nombre));
        await EliminarConConfirmacionAsync(page, confirmar: true);
        await Assertions.Expect(page.Locator(".catalogo__grilla article")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync(new Regex("carrit|asociad|relacionad", RegexOptions.IgnoreCase));
        Assert.NotNull(await fixture.ObtenerProductoAsync(producto.IdProducto));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Administracion_NoExponeAccionesNiPermiteEditarSinRolAdmin(bool clienteAutenticado)
    {
        var producto = await fixture.CrearProductoAsync($"Permisos-{Guid.NewGuid():N}");
        await using var context = await fixture.Browser.NewContextAsync();
        var page = clienteAutenticado ? await IniciarSesionAsync(context, administrador: false) : await context.NewPageAsync();
        await page.GotoAsync(Catalogo(producto.Nombre));
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Editar", Exact = true })).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true })).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Nuevo producto" })).ToHaveCountAsync(0);
        await page.GotoAsync($"{fixture.FrontendUrl}/Productos/Editar/{producto.IdProducto}");
        Assert.Contains("/Home/Login", page.Url);
        Assert.Equal(producto.Nombre, (await fixture.ObtenerProductoAsync(producto.IdProducto))?.Nombre);
    }

    [Fact]
    public async Task Administracion_RechazaMutacionesSinCsrfYEdicionDeProductoInexistente()
    {
        var producto = await fixture.CrearProductoAsync($"Csrf-{Guid.NewGuid():N}");
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await IniciarSesionAsync(context, administrador: true);
        foreach (var accion in new[] { "Editar", "Eliminar" })
        {
            var response = await context.APIRequest.PostAsync($"{fixture.FrontendUrl}/Productos/{accion}/{producto.IdProducto}");
            Assert.Equal(400, response.Status);
        }
        Assert.Equal(producto.Nombre, (await fixture.ObtenerProductoAsync(producto.IdProducto))?.Nombre);
        var inexistente = await page.GotoAsync($"{fixture.FrontendUrl}/Productos/Editar/2147483647");
        Assert.Equal(404, inexistente?.Status);
    }

    private string Catalogo(string texto) => $"{fixture.FrontendUrl}/Productos?texto={Uri.EscapeDataString(texto)}";

    private string CatalogoFiltrado(string texto, int idCategoria, int pagina)
        => $"{Catalogo(texto)}&idCategoria={idCategoria}&precioMin=100&precioMax=300&soloDisponibles=true&pagina={pagina}&tamanoPagina=12";

    private async Task<List<Totaltech.Entidades.Producto>> CrearPaginaCompletaYUnaTarjetaAsync(string clave)
    {
        var productos = new List<Totaltech.Entidades.Producto>();
        for (var indice = 0; indice < 13; indice++)
            productos.Add(await fixture.CrearProductoAsync($"{clave}-{indice:D2}"));
        return productos;
    }

    private async Task<IPage> IniciarSesionAsync(IBrowserContext context, bool administrador)
    {
        var (email, contrasena) = administrador
            ? await fixture.CrearAdministradorAsync() : await fixture.CrearClienteAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.FrontendUrl}/Home/Login");
        await page.GetByLabel("Email", new() { Exact = true }).FillAsync(email);
        await page.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(contrasena);
        await page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();
        return page;
    }

    private static Task AbrirEdicionAsync(IPage page)
        => page.Locator(".catalogo__grilla article").GetByRole(AriaRole.Link, new() { Name = "Editar", Exact = true }).ClickAsync();

    private static Task GuardarAsync(IPage page)
        => page.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();

    private static Task SeleccionarPngAsync(IPage page)
        => page.GetByLabel("Imagen de referencia", new() { Exact = true }).SetInputFilesAsync(
            new FilePayload { Name = "referencia.png", MimeType = "image/png", Buffer = Png });

    private static async Task SinDesbordeHorizontalAsync(IPage page)
        => Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth"),
            $"Desborde horizontal en {page.Url} con ancho {page.ViewportSize?.Width}.");

    private static async Task CapturarAsync(IPage page, string nombre)
    {
        var carpeta = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../TestResults/catalogo-admin"));
        Directory.CreateDirectory(carpeta);
        await page.ScreenshotAsync(new() { Path = Path.Combine(carpeta, nombre), FullPage = true });
    }

    private static async Task<string> EliminarConConfirmacionAsync(IPage page, bool confirmar)
    {
        string? mensaje = null;
        Task respuesta = Task.CompletedTask;
        void ResolverDialogo(object? sender, IDialog dialogo)
        {
            mensaje = dialogo.Message;
            respuesta = confirmar ? dialogo.AcceptAsync() : dialogo.DismissAsync();
        }
        page.Dialog += ResolverDialogo;
        try
        {
            await page.Locator(".catalogo__grilla article").GetByRole(AriaRole.Button, new() { Name = "Eliminar", Exact = true }).ClickAsync();
            await respuesta;
            Assert.False(string.IsNullOrWhiteSpace(mensaje), "La eliminación debe pedir confirmación.");
            return mensaje!;
        }
        finally { page.Dialog -= ResolverDialogo; }
    }
}
