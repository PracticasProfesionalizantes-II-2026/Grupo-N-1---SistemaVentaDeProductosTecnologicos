using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging.Abstractions;
using Totaltech.IntegrationTests.Infrastructure;
using Totaltech.Logica;
using Totaltech.Repositorios;
using Entidades = Totaltech.Entidades;

namespace Totaltech.IntegrationTests.E2E;

public sealed class CatalogoE2EFixture : IAsyncLifetime
{
    private readonly SqlServerTestDatabase _database = new();
    private readonly List<Process> _procesos = [];
    private readonly Dictionary<Process, (Task<string> Salida, Task<string> Error)> _salidas = [];
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private Process? _frontend;
    private string _raiz = string.Empty;
    private string _backendUrl = string.Empty;
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), $"TotaltechE2E_{Guid.NewGuid():N}");

    public string FrontendUrl { get; private set; } = string.Empty;
    public IBrowser Browser => _browser ?? throw new InvalidOperationException("El navegador no está iniciado.");

    public async Task<(string Email, string Contrasena)> CrearClienteAsync()
        => await CrearUsuarioAsync(Entidades.RolUsuario.Cliente);

    public async Task<(string Email, string Contrasena)> CrearAdministradorAsync()
        => await CrearUsuarioAsync(Entidades.RolUsuario.Administrador);

    private async Task<(string Email, string Contrasena)> CrearUsuarioAsync(Entidades.RolUsuario rol)
    {
        var email = $"catalogo-{Guid.NewGuid():N}@test.local";
        const string contrasena = "Catalogo123456";
        await using var db = _database.CreateContext();
        var logica = new UsuariosLogica(new UsuariosRepositorio(db));
        var error = await logica.CrearAsync(new Entidades.Usuario
        {
            Nombre = "Usuario", Apellido = "E2E", Email = email, Contrasena = contrasena,
            Telefono = "1111111111", FechaRegistro = DateTime.UtcNow, Rol = rol
        });
        if (error is not null) throw new InvalidOperationException(error);
        return (email, contrasena);
    }

    public async Task<Entidades.Usuario> ObtenerUsuarioAsync(string email)
    {
        await using var db = _database.CreateContext();
        return await db.Usuarios.AsNoTracking().SingleAsync(u => u.Email == email);
    }

    public async Task<Entidades.Producto> CrearProductoAsync(string nombre, string? descripcion = null)
    {
        await using var db = _database.CreateContext();
        var producto = new Entidades.Producto
        {
            Nombre = nombre, Descripcion = descripcion ?? "Producto exclusivo de una prueba E2E",
            Precio = 250, Stock = 8,
            IdCategoria = await db.Categorias.Select(categoria => categoria.IdCategoria).FirstAsync(),
            IdProveedor = await db.Proveedores.Where(proveedor => proveedor.Activo)
                .Select(proveedor => proveedor.IdProveedor).FirstAsync()
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }

    public async Task<Entidades.Producto?> ObtenerProductoAsync(int id)
    {
        await using var db = _database.CreateContext();
        return await db.Productos.AsNoTracking().SingleOrDefaultAsync(producto => producto.IdProducto == id);
    }

    public async Task AsociarCarritoAsync(int idProducto)
    {
        var (email, _) = await CrearClienteAsync();
        await using var db = _database.CreateContext();
        var usuario = await db.Usuarios.SingleAsync(item => item.Email == email);
        var carrito = new Entidades.Carrito
        {
            IdUsuario = usuario.IdUsuario, FechaCreacion = DateTime.UtcNow,
            Estado = Entidades.EstadoCarrito.Activo
        };
        db.DetalleCarritos.Add(new Entidades.DetalleCarrito
        {
            Carrito = carrito, IdProducto = idProducto, Cantidad = 1, PrecioUnitario = 250, Subtotal = 250
        });
        await db.SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
        await SembrarCatalogoAsync();
        _raiz = EncontrarRaiz();
        CopiarWebRoot();
        var backendPort = PuertoLibre();
        var frontendPort = PuertoLibre();
        _backendUrl = $"http://127.0.0.1:{backendPort}";
        FrontendUrl = $"http://127.0.0.1:{frontendPort}";

        var backend = Iniciar(_raiz, "Totaltech/Totaltech.csproj", new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development", ["ASPNETCORE_URLS"] = _backendUrl,
            ["ConnectionStrings__DefaultConnection"] = _database.ConnectionString,
            ["Database__ApplyMigrations"] = "false", ["DemoData__Enabled"] = "false",
            ["BootstrapAdmin__Enabled"] = "false",
            ["Authentication__Issuer"] = "E2E", ["Authentication__Audience"] = "E2E",
            ["Authentication__SigningKey"] = "e2e-tests-signing-key-with-32-bytes-minimum"
        });
        _procesos.Add(backend);
        await EsperarAsync($"{_backendUrl}/productos/catalogo", backend);
        await IniciarFrontendAsync();
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task ReiniciarFrontendAsync()
    {
        if (_frontend is not null) await DetenerAsync(_frontend);
        await IniciarFrontendAsync();
    }

    private async Task IniciarFrontendAsync()
    {
        _frontend = Iniciar(_raiz, "Frontend/Frontend.csproj", new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development", ["ASPNETCORE_URLS"] = FrontendUrl,
            ["ASPNETCORE_WEBROOT"] = _webRoot, ["ApiBaseUrl"] = _backendUrl
        });
        _procesos.Add(_frontend);
        await EsperarAsync($"{FrontendUrl}/Productos", _frontend);
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
        foreach (var proceso in _procesos) await DetenerAsync(proceso);
        foreach (var proceso in _procesos) proceso.Dispose();
        await _database.DisposeAsync();
        var destino = Path.GetFullPath(_webRoot);
        var temporal = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!destino.StartsWith(temporal, StringComparison.OrdinalIgnoreCase)
            || !Path.GetFileName(destino).StartsWith("TotaltechE2E_", StringComparison.Ordinal))
            throw new InvalidOperationException("Sólo se puede eliminar el directorio temporal de esta fixture.");
        if (Directory.Exists(destino)) Directory.Delete(destino, recursive: true);
    }

    private Process Iniciar(string raiz, string proyecto, Dictionary<string, string> entorno)
    {
        // Ejecutar la misma configuración que el ensamblado de pruebas.
#if DEBUG
        const string configuracion = "Debug";
#else
        const string configuracion = "Release";
#endif
        var info = new ProcessStartInfo("dotnet", $"run --no-build --no-launch-profile --configuration {configuracion} --project {proyecto}")
        { WorkingDirectory = raiz, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var item in entorno) info.Environment[item.Key] = item.Value;
        var proceso = Process.Start(info) ?? throw new InvalidOperationException($"No se pudo iniciar {proyecto}.");
        _salidas[proceso] = (proceso.StandardOutput.ReadToEndAsync(), proceso.StandardError.ReadToEndAsync());
        return proceso;
    }

    private async Task EsperarAsync(string url, Process proceso)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (var intento = 0; intento < 60; intento++)
        {
            if (proceso.HasExited)
                throw new InvalidOperationException($"El proceso terminó al iniciar. {await _salidas[proceso].Error} {await _salidas[proceso].Salida}");
            try { using var response = await client.GetAsync(url); if (response.IsSuccessStatusCode) return; }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException($"El servicio no respondió en {url}.");
    }

    private static async Task DetenerAsync(Process proceso)
    {
        if (!proceso.HasExited) proceso.Kill(entireProcessTree: true);
        await proceso.WaitForExitAsync();
    }

    private void CopiarWebRoot()
    {
        var origen = Path.Combine(_raiz, "Frontend", "wwwroot");
        Directory.CreateDirectory(_webRoot);
        foreach (var archivo in Directory.EnumerateFiles(origen, "*", SearchOption.AllDirectories))
        {
            var destino = Path.Combine(_webRoot, Path.GetRelativePath(origen, archivo));
            Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
            File.Copy(archivo, destino);
        }
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
