using Frontend.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

public sealed class GarantiaUiTests
{
    [Fact]
    public async Task Garantia_Publica_ValidaConfirmaSinEnviar_ConTecladoYSeisAnchos()
    {
        // TestServer ejecuta MVC/Razor reales sin iniciar procesos ni utilizar la API o SQL.
        using var factory = new WebApplicationFactory<ConsultasController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services => services.AddSingleton<IHttpClientFactory, ApiProhibida>());
        });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://totaltech.test"), AllowAutoRedirect = false });
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.RouteAsync("https://totaltech.test/**", async route =>
        {
            using var response = await client.GetAsync(route.Request.Url);
            await route.FulfillAsync(new()
            {
                Status = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream",
                BodyBytes = await response.Content.ReadAsByteArrayAsync()
            });
        });
        await page.GotoAsync("https://totaltech.test/Home/Login");
        Assert.Equal(0, await page.GetByRole(AriaRole.Link, new() { Name = "Disponibles", Exact = true }).CountAsync());
        await page.GetByRole(AriaRole.Link, new() { Name = "Garantía", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync("https://totaltech.test/Consultas/Garantia");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 }))
            .ToHaveTextAsync("Consultá por la garantía de tu producto");
        var requests = new List<string>();
        page.Request += (_, request) => requests.Add(request.Url);
        var nombre = page.Locator("#garantia-nombre");
        var email = page.Locator("#garantia-email");
        var producto = page.Locator("#garantia-producto");
        var fecha = page.Locator("#garantia-fecha");
        var problema = page.Locator("#garantia-problema");
        var enviar = page.GetByRole(AriaRole.Button, new() { Name = "Enviar consulta", Exact = true });
        var dialog = page.GetByRole(AriaRole.Dialog);
        await enviar.ClickAsync();
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
        await Assertions.Expect(nombre).ToBeFocusedAsync();
        Assert.Equal(5, await page.Locator("[aria-invalid=true]").CountAsync());

        await nombre.FillAsync("   ");
        await email.FillAsync("email-invalido");
        await producto.FillAsync("   ");
        await problema.FillAsync("   ");
        await enviar.ClickAsync();
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#garantia-email-error")).ToHaveTextAsync("Ingresá un email válido.");
        await Assertions.Expect(nombre).ToBeFocusedAsync();
        await nombre.FillAsync("Facundo");
        await email.FillAsync("facundo@example.com");
        await producto.FillAsync("Notebook Lenovo");
        await problema.FillAsync("La pantalla no enciende.");
        await fecha.FillAsync("2999-01-01");
        await enviar.ClickAsync();
        await Assertions.Expect(fecha).ToBeFocusedAsync();
        await Assertions.Expect(page.Locator("#garantia-fecha-error")).ToHaveTextAsync("La fecha de compra no puede ser futura.");
        await Assertions.Expect(dialog).Not.ToBeVisibleAsync();

        foreach (var width in new[] { 320, 375, 425, 768, 1024, 1280 })
        {
            await page.SetViewportSizeAsync(width, 900);
            await nombre.FillAsync("Facundo");
            await email.FillAsync("facundo@example.com");
            await producto.FillAsync("Notebook Lenovo");
            await fecha.FillAsync(await fecha.GetAttributeAsync("max") ?? throw new InvalidOperationException("Falta fecha máxima."));
            await problema.FillAsync("La pantalla no enciende.");
            if (width >= 768)
            {
                await page.Locator("#garantia-telefono").FillAsync("+54 11 1234 5678");
                await page.Locator("#garantia-serie").FillAsync("ABC-1234");
            }
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
            var first = await nombre.BoundingBoxAsync();
            var second = await email.BoundingBoxAsync();
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.True(width >= 768 ? Math.Abs(first!.Y - second!.Y) < 1 : second!.Y > first!.Y);
            await CapturarAsync(page, $"formulario-{width}.png");
            await enviar.FocusAsync();
            await page.Keyboard.PressAsync("Enter");
            await Assertions.Expect(dialog).ToBeVisibleAsync();
            await Assertions.Expect(dialog.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Consulta enviada");
            await Assertions.Expect(page.Locator("#garantia-confirmacion-texto"))
                .ToHaveTextAsync("Gracias por contactarnos. En breve nos pondremos en contacto con vos para orientarte sobre la gestión de la garantía con el proveedor o el soporte oficial del fabricante.");
            var aceptar = dialog.GetByRole(AriaRole.Button, new() { Name = "Aceptar", Exact = true });
            await Assertions.Expect(aceptar).ToBeFocusedAsync();
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(aceptar).ToBeFocusedAsync();
            foreach (var input in new[] { nombre, email, producto, fecha, problema, page.Locator("#garantia-telefono"), page.Locator("#garantia-serie") })
                await Assertions.Expect(input).ToHaveValueAsync("");
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));
            await CapturarAsync(page, $"confirmacion-{width}.png");
            if (width == 320) await page.Keyboard.PressAsync("Escape");
            else await aceptar.ClickAsync();
            await Assertions.Expect(dialog).Not.ToBeVisibleAsync();
            await Assertions.Expect(enviar).ToBeFocusedAsync();
        }
        Assert.Empty(requests);
        await page.GetByRole(AriaRole.Link, new() { Name = "Contactos", Exact = true }).ClickAsync();
        await ContactoE2ETests.VerificarFormularioAsync(page);
        using var disponibles = await client.GetAsync("/Productos/Disponibles");
        Assert.Equal("/Productos?soloDisponibles=True", disponibles.Headers.Location?.OriginalString);
    }

    private static async Task CapturarAsync(IPage page, string name)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "TestResults", "garantia");
        Directory.CreateDirectory(directory);
        await page.ScreenshotAsync(new() { Path = Path.Combine(directory, name), FullPage = true });
    }

    private sealed class ApiProhibida : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => throw new InvalidOperationException("El formulario de demostración no debe acceder a la API.");
    }
}
