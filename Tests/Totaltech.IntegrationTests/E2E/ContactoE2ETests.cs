using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

[Trait("Category", "E2E")]
public sealed class ContactoE2ETests : IAsyncLifetime
{
    private Process? _frontend;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private string _frontendUrl = string.Empty;

    public async Task InitializeAsync()
    {
        var raiz = new DirectoryInfo(AppContext.BaseDirectory);
        while (raiz is not null && !File.Exists(Path.Combine(raiz.FullName, "Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln")))
            raiz = raiz.Parent;
        if (raiz is null) throw new DirectoryNotFoundException("No se encontró la solución.");
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        _frontendUrl = $"http://127.0.0.1:{puerto}";

        var configuracion = Directory.GetParent(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))!.Name;
        var inicio = new ProcessStartInfo("dotnet", $"run --no-build --no-launch-profile -c {configuracion} --project Frontend/Frontend.csproj")
        {
            WorkingDirectory = raiz.FullName, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        inicio.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        inicio.Environment["ASPNETCORE_URLS"] = _frontendUrl;
        // Esta página de demostración debe funcionar sin un backend disponible.
        inicio.Environment["ApiBaseUrl"] = "http://127.0.0.1:1";
        _frontend = Process.Start(inicio) ?? throw new InvalidOperationException("No se pudo iniciar el frontend.");
        var salida = _frontend.StandardOutput.ReadToEndAsync();
        var error = _frontend.StandardError.ReadToEndAsync();
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        var disponible = false;
        for (var intento = 0; intento < 60; intento++)
        {
            if (_frontend.HasExited) throw new InvalidOperationException($"El frontend terminó: {await error} {await salida}");
            try
            {
                using var respuesta = await client.GetAsync($"{_frontendUrl}/Consultas/Contacto");
                if (respuesta.IsSuccessStatusCode) { disponible = true; break; }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }
            await Task.Delay(500);
        }
        if (!disponible) throw new TimeoutException("El formulario de contacto no respondió.");
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
        if (_frontend is not null)
        {
            if (!_frontend.HasExited) _frontend.Kill(entireProcessTree: true);
            await _frontend.WaitForExitAsync();
            _frontend.Dispose();
        }
    }

    [Fact]
    public async Task Contactos_ValidaMuestraConfirmacionYLimpiaSinEnviarSolicitudes()
    {
        await using var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{_frontendUrl}/Home/Login");
        await page.GetByRole(AriaRole.Link, new() { Name = "Contactos", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync($"{_frontendUrl}/Consultas/Contacto");
        await VerificarFormularioAsync(page);
    }

    internal static async Task VerificarFormularioAsync(IPage page)
    {
        var solicitudes = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST" || request.Url.Contains("/consultas", StringComparison.OrdinalIgnoreCase))
                solicitudes.Add(request.Url);
        };
        var nombre = page.GetByLabel("Nombre", new() { Exact = true });
        var email = page.GetByLabel("Email", new() { Exact = true });
        var mensaje = page.GetByLabel("Mensaje", new() { Exact = true });
        var enviar = page.GetByRole(AriaRole.Button, new() { Name = "Enviar", Exact = true });
        var dialog = page.GetByRole(AriaRole.Dialog);
        await enviar.ClickAsync();
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
        await Assertions.Expect(nombre).ToBeFocusedAsync();
        await Assertions.Expect(page.Locator("#contacto-nombre-error")).ToHaveTextAsync("Ingresá tu nombre.");

        await nombre.FillAsync("   ");
        await email.FillAsync("email-invalido");
        await mensaje.FillAsync("   ");
        await enviar.ClickAsync();
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#contacto-email-error")).ToHaveTextAsync("Ingresá un email válido.");
        await Assertions.Expect(page.Locator("#contacto-mensaje-error")).ToHaveTextAsync("Escribí tu mensaje.");

        foreach (var width in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(width, 900);
            await nombre.FillAsync("Facundo");
            await email.FillAsync("facundo@example.com");
            await mensaje.FillAsync("Quisiera consultar por un producto.");
            Assert.True(await page.Locator("body").EvaluateAsync<bool>("el => el.scrollWidth <= el.clientWidth"));
            if (width is 320 or 1280) await CapturarAsync(page, $"formulario-{width}.png");
            await enviar.FocusAsync();
            await page.Keyboard.PressAsync("Enter");
            await Assertions.Expect(dialog).ToBeVisibleAsync();
            await Assertions.Expect(dialog.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Mensaje enviado");
            await Assertions.Expect(dialog.GetByText("Gracias por contactarnos", new() { Exact = true })).ToBeVisibleAsync();
            var aceptar = dialog.GetByRole(AriaRole.Button, new() { Name = "Aceptar", Exact = true });
            await Assertions.Expect(aceptar).ToBeFocusedAsync();
            await Assertions.Expect(nombre).ToHaveValueAsync("");
            await Assertions.Expect(email).ToHaveValueAsync("");
            await Assertions.Expect(mensaje).ToHaveValueAsync("");
            Assert.True(await dialog.EvaluateAsync<bool>("el => el.getBoundingClientRect().width <= window.innerWidth"));
            if (width is 320 or 1280) await CapturarAsync(page, $"confirmacion-{width}.png");
            if (width == 320) await page.Keyboard.PressAsync("Escape");
            else await aceptar.ClickAsync();
            await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
            await Assertions.Expect(enviar).ToBeFocusedAsync();
        }
        Assert.Empty(solicitudes);
    }

    private static async Task CapturarAsync(IPage page, string nombre)
    {
        var carpeta = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../TestResults/contacto"));
        Directory.CreateDirectory(carpeta);
        await page.ScreenshotAsync(new() { Path = Path.Combine(carpeta, nombre), FullPage = true });
    }
}
