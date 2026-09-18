using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging.Abstractions;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica;
using Totaltech.Repositorios;

namespace Totaltech.IntegrationTests.E2E;

public sealed class CatalogoE2EFixture : IAsyncLifetime
{
    private readonly SqlServerTestDatabase _database = new();
    private readonly List<Process> _procesos = [];
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string FrontendUrl { get; private set; } = string.Empty;
    public IBrowser Browser => _browser ?? throw new InvalidOperationException("El navegador no está iniciado.");

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
        await SembrarCatalogoAsync();
        var raiz = EncontrarRaiz();
        var backendPort = PuertoLibre();
        var frontendPort = PuertoLibre();
        var backendUrl = $"http://127.0.0.1:{backendPort}";
        FrontendUrl = $"http://127.0.0.1:{frontendPort}";

        var backend = Iniciar(raiz, "Totaltech/Totaltech.csproj", new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development", ["ASPNETCORE_URLS"] = backendUrl,
            ["ConnectionStrings__DefaultConnection"] = _database.ConnectionString,
            ["Database__ApplyMigrations"] = "false", ["DemoData__Enabled"] = "false",
            ["BootstrapAdmin__Enabled"] = "false",
            ["Authentication__Issuer"] = "E2E", ["Authentication__Audience"] = "E2E",
            ["Authentication__SigningKey"] = "e2e-tests-signing-key-with-32-bytes-minimum"
        });
        _procesos.Add(backend);
        await EsperarAsync($"{backendUrl}/productos/catalogo", backend);

        var frontend = Iniciar(raiz, "Frontend/Frontend.csproj", new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development", ["ASPNETCORE_URLS"] = FrontendUrl,
            ["ApiBaseUrl"] = backendUrl
        });
        _procesos.Add(frontend);
        await EsperarAsync($"{FrontendUrl}/Productos", frontend);
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
        foreach (var proceso in _procesos.Where(p => !p.HasExited)) proceso.Kill(true);
        foreach (var proceso in _procesos) proceso.Dispose();
        await _database.DisposeAsync();
    }

    private static Process Iniciar(string raiz, string proyecto, Dictionary<string, string> entorno)
    {
        var info = new ProcessStartInfo("dotnet", $"run --no-build --no-launch-profile --configuration Release --project {proyecto}")
        { WorkingDirectory = raiz, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var item in entorno) info.Environment[item.Key] = item.Value;
        return Process.Start(info) ?? throw new InvalidOperationException($"No se pudo iniciar {proyecto}.");
    }

    private static async Task EsperarAsync(string url, Process proceso)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (var intento = 0; intento < 60; intento++)
        {
            if (proceso.HasExited)
                throw new InvalidOperationException($"El proceso terminó al iniciar. {await proceso.StandardError.ReadToEndAsync()} {await proceso.StandardOutput.ReadToEndAsync()}");
            try { using var response = await client.GetAsync(url); if (response.IsSuccessStatusCode) return; }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException($"El servicio no respondió en {url}.");
    }

    private static int PuertoLibre()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop(); return port;
    }

    private async Task SembrarCatalogoAsync()
    {
        await using var db = _database.CreateContext();
        var categorias = new CategoriasLogica(new CategoriasRepositorio(db));
        var proveedoresRepositorio = new ProveedoresRepositorio(db);
        var productosRepositorio = new ProductosRepositorio(db);
        await CategoriasIniciales.InicializarAsync(categorias);
        await CatalogoDemostracionIniciales.InicializarAsync(
            categorias,
            new ProveedoresLogica(proveedoresRepositorio),
            new ProductosLogica(productosRepositorio, new CategoriasRepositorio(db), proveedoresRepositorio),
            NullLogger.Instance);
    }

    private static string EncontrarRaiz()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Grupo-N-1---SistemaVentaDeProductosTecnologicos.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("No se encontró la raíz de la solución.");
    }
}

[CollectionDefinition("Catalogo E2E")]
public sealed class CatalogoE2ECollection : ICollectionFixture<CatalogoE2EFixture>;
