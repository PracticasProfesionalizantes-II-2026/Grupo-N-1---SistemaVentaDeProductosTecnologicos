using Microsoft.Playwright;

namespace Totaltech.IntegrationTests.E2E;

public sealed class ContactoUiTests
{
    [Fact]
    public async Task FormularioReal_ValidaCamposDialogoTecladoYSeisAnchosSinServidor()
    {
        var raiz = new DirectoryInfo(AppContext.BaseDirectory);
        while (raiz is not null && !File.Exists(Path.Combine(raiz.FullName, "Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln")))
            raiz = raiz.Parent;
        Assert.NotNull(raiz);
        var frontend = Path.Combine(raiz!.FullName, "Frontend");
        var vista = await File.ReadAllTextAsync(Path.Combine(frontend, "Views/Consultas/Contacto.cshtml"));
        // El formulario es HTML estático: se prueba exactamente el marcado de la vista y sus recursos.
        var marcado = vista[vista.IndexOf("<section", StringComparison.Ordinal)..vista.IndexOf("@section Scripts", StringComparison.Ordinal)];
        var bootstrap = await File.ReadAllTextAsync(Path.Combine(frontend, "wwwroot/lib/bootstrap/dist/css/bootstrap.min.css"));
        var estilos = await File.ReadAllTextAsync(Path.Combine(frontend, "wwwroot/css/site.css"));
        var script = await File.ReadAllTextAsync(Path.Combine(frontend, "wwwroot/js/contacto.js"));
        using var playwright = await Playwright.CreateAsync();
        await using var navegador = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await navegador.NewPageAsync();
        await page.SetContentAsync($"<html lang='es'><head><meta name='viewport' content='width=device-width, initial-scale=1'><style>{bootstrap}\n{estilos}</style></head><body>{marcado}<script>{script}</script></body></html>");
        await ContactoE2ETests.VerificarFormularioAsync(page);
    }
}
