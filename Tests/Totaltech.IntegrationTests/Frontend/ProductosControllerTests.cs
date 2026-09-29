using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Frontend.Controllers;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ProductosControllerTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), $"totaltech-mvc-imagenes-{Guid.NewGuid():N}");
    private readonly ApiSimulada _api = new();
    private readonly HttpClient _cliente;

    public ProductosControllerTests()
    {
        _cliente = new HttpClient(_api) { BaseAddress = new Uri("https://totaltech.test") };
    }

    [Fact]
    public async Task RenombrarSinArchivoConservaLaImagenOriginalYRegresaConLosFiltros()
    {
        _api.Producto.ImagenUrl = null;
        var controller = CrearController();
        var modelo = CrearEdicion();
        modelo.Nombre = "Notebook renombrada";
        modelo.ReturnUrl = "/Productos?texto=Notebook&pagina=2&tamanoPagina=12";

        var resultado = Assert.IsType<LocalRedirectResult>(await controller.Editar(7, modelo));

        using var payload = JsonDocument.Parse(Assert.Single(_api.Actualizaciones));
        Assert.Equal("Notebook renombrada", payload.RootElement.GetProperty("nombre").GetString());
        Assert.Equal(new ProductoImagenResolver().Resolver(_api.Producto.Nombre), payload.RootElement.GetProperty("imagenUrl").GetString());
        Assert.False(payload.RootElement.TryGetProperty("imagenArchivo", out _));
        Assert.Equal(modelo.ReturnUrl, resultado.Url);
        Assert.Equal("Producto actualizado correctamente.", controller.TempData["Mensaje"]);
        Assert.False(Directory.Exists(_raiz));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RechazoDeApiRetiraImagenNuevaPeroTimeoutLaConserva(bool timeout)
    {
        _api.TimeoutAlActualizar = timeout;
        _api.EstadoActualizar = HttpStatusCode.BadRequest;
        var controller = CrearController();
        var modelo = CrearEdicion();
        modelo.ImagenArchivo = ArchivoPng();
        var imagenAnterior = _api.Producto.ImagenUrl;

        var vista = Assert.IsType<ViewResult>(await controller.Editar(7, modelo));

        using var payload = JsonDocument.Parse(Assert.Single(_api.Actualizaciones));
        var nuevaImagen = payload.RootElement.GetProperty("imagenUrl").GetString()!;
        Assert.StartsWith("/uploads/productos/", nuevaImagen);
        Assert.Equal(timeout, File.Exists(RutaImagen(nuevaImagen)));
        Assert.Same(modelo, vista.Model);
        Assert.Equal(imagenAnterior, modelo.ImagenActualUrl);
        Assert.Equal("Nombre modificado", modelo.Nombre);
        Assert.Contains(Errores(controller), mensaje => mensaje.Contains("seleccionar el archivo"));
        Assert.Contains(Errores(controller), mensaje => mensaje.Contains(timeout ? "confirmar el guardado" : "rechazada por la API"));
        Assert.NotNull(controller.ViewBag.Categorias);
        Assert.NotNull(controller.ViewBag.Proveedores);
    }

    [Fact]
    public async Task ImagenActualSuministradaEnInputSeReemplazaPorLaPersistida()
    {
        var controller = CrearController();
        var modelo = CrearEdicion();
        modelo.ImagenActualUrl = "/uploads/productos/imagen-ajena.png";

        Assert.IsType<LocalRedirectResult>(await controller.Editar(7, modelo));

        using var payload = JsonDocument.Parse(Assert.Single(_api.Actualizaciones));
        Assert.Equal(_api.Producto.ImagenUrl, payload.RootElement.GetProperty("imagenUrl").GetString());
        Assert.Equal(_api.Producto.ImagenUrl, modelo.ImagenActualUrl);
    }

    [Theory]
    [InlineData("https://example.com/Productos")]
    [InlineData("//example.com/Productos")]
    [InlineData("/Proveedores")]
    public async Task RetornoExternoOFueraDelCatalogoUsaLaRutaDelCatalogo(string returnUrl)
    {
        var controller = CrearController();
        var modelo = CrearEdicion();
        modelo.ReturnUrl = returnUrl;

        var resultado = Assert.IsType<LocalRedirectResult>(await controller.Editar(7, modelo));

        Assert.Equal("/Productos", resultado.Url);
    }

    [Fact]
    public async Task ArchivoConFirmaInvalidaNoEnviaPutYPreservaDatos()
    {
        var controller = CrearController();
        var modelo = CrearEdicion();
        modelo.ImagenArchivo = new FormFile(new MemoryStream("no es una imagen"u8.ToArray()), 0, 16, "ImagenArchivo", "foto.png")
        {
            Headers = new HeaderDictionary(), ContentType = "image/png"
        };

        var vista = Assert.IsType<ViewResult>(await controller.Editar(7, modelo));

        Assert.Same(modelo, vista.Model);
        Assert.Empty(_api.Actualizaciones);
        Assert.Equal(_api.Producto.ImagenUrl, modelo.ImagenActualUrl);
        Assert.Equal("Nombre modificado", modelo.Nombre);
        Assert.Contains(Errores(controller), mensaje => mensaje.Contains("contenido del archivo"));
        Assert.False(Directory.Exists(_raiz));
    }

    [Fact]
    public async Task EliminarConRelacionesExplicaElConflictoYConservaElRegresoLocal()
    {
        _api.EstadoEliminar = HttpStatusCode.Conflict;
        var controller = CrearController();
        const string regreso = "/Productos/Index?texto=Notebook&pagina=3";

        var resultado = Assert.IsType<LocalRedirectResult>(await controller.Eliminar(7, regreso));

        Assert.Equal(regreso, resultado.Url);
        Assert.Contains("pedidos, carritos u otros datos relacionados", Assert.IsType<string>(controller.TempData["Error"]));
        Assert.Null(controller.TempData["Mensaje"]);
        Assert.Equal(1, _api.Eliminaciones);
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(0, 1)]
    public async Task AdministradorEnPaginaFueraDeRangoRegresaAUltimaPaginaConLosFiltros(int totalPaginas, int esperada)
    {
        _api.Catalogo.TotalPaginas = totalPaginas;
        var controller = CrearController();

        var resultado = Assert.IsType<RedirectToActionResult>(await controller.Index("Notebook", 4, 100, 500, true, 3, 12));

        Assert.Equal("Index", resultado.ActionName);
        Assert.Equal(esperada, resultado.RouteValues!["pagina"]);
        Assert.Equal("Notebook", resultado.RouteValues["texto"]);
        Assert.Equal(4, resultado.RouteValues["idCategoria"]);
        Assert.Equal(100m, resultado.RouteValues["precioMin"]);
        Assert.Equal(500m, resultado.RouteValues["precioMax"]);
        Assert.Equal(true, resultado.RouteValues["soloDisponibles"]);
        Assert.Equal(12, resultado.RouteValues["tamanoPagina"]);
    }

    [Fact]
    public async Task ProductoInexistenteDevuelveVista404SinActualizar()
    {
        _api.ProductoInexistente = true;
        var controller = CrearController();

        var resultado = Assert.IsType<ViewResult>(await controller.Editar(7, CrearEdicion()));

        Assert.Equal("NoEncontrado", resultado.ViewName);
        Assert.Equal(StatusCodes.Status404NotFound, controller.Response.StatusCode);
        Assert.Empty(_api.Actualizaciones);
    }

    private ProductosController CrearController()
    {
        var factory = new ClienteFactory(_cliente);
        var controller = new ProductosController(new ProductosApiService(factory, new ProductoImagenResolver()),
            new CategoriasApiService(factory), new ProveedoresApiService(factory),
            new ProductoImagenStorage(new EntornoWeb { ContentRootPath = _raiz, WebRootPath = Path.Combine(_raiz, "wwwroot") }),
            NullLogger<ProductosController>.Instance);
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")], "Tests"))
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new TempDataMemoria());
        controller.Url = new UrlHelperSimulado(controller.ControllerContext);
        return controller;
    }

    private static ProductoEdicionViewModel CrearEdicion() => new()
    {
        Nombre = "Nombre modificado", Descripcion = "Descripción modificada", Precio = 200, Stock = 4,
        IdCategoria = 1, IdProveedor = 1, ReturnUrl = "/Productos"
    };

    private static FormFile ArchivoPng()
    {
        var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aAfkAAAAASUVORK5CYII=");
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "ImagenArchivo", "foto.png")
        {
            Headers = new HeaderDictionary(), ContentType = "image/png"
        };
    }

    private static IEnumerable<string> Errores(Controller controller) =>
        controller.ModelState.Values.SelectMany(valor => valor.Errors).Select(error => error.ErrorMessage);

    private string RutaImagen(string url) => Path.Combine(_raiz, "wwwroot", "uploads", "productos", Path.GetFileName(url));

    public void Dispose()
    {
        _cliente.Dispose();
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
    }

    private sealed class ClienteFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class ApiSimulada : HttpMessageHandler
    {
        public ProductoResponse Producto { get; } = new()
        {
            IdProducto = 7, Nombre = "Notebook Lenovo LOQ 15", Descripcion = "Descripción original", Precio = 100,
            Stock = 2, IdCategoria = 1, IdProveedor = 1, ImagenUrl = "/uploads/productos/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png"
        };
        public CatalogoProductosResponse Catalogo { get; } = new() { Pagina = 3, TamanoPagina = 12, TotalItems = 24, TotalPaginas = 2 };
        public HttpStatusCode EstadoActualizar { get; set; } = HttpStatusCode.NoContent;
        public HttpStatusCode EstadoEliminar { get; set; } = HttpStatusCode.NoContent;
        public bool TimeoutAlActualizar { get; set; }
        public bool ProductoInexistente { get; set; }
        public List<string> Actualizaciones { get; } = [];
        public int Eliminaciones { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Put)
            {
                Actualizaciones.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
                if (TimeoutAlActualizar) throw new TaskCanceledException("Timeout simulado.");
                return new HttpResponseMessage(EstadoActualizar) { Content = JsonContent.Create("Actualización rechazada por la API.") };
            }
            if (request.Method == HttpMethod.Delete)
            {
                Eliminaciones++;
                return new HttpResponseMessage(EstadoEliminar);
            }
            return request.RequestUri!.AbsolutePath.TrimEnd('/') switch
            {
                "/productos/7" => ProductoInexistente ? new(HttpStatusCode.NotFound) : Json(Producto),
                "/productos/catalogo" => Json(Catalogo),
                "/categorias" => Json(new[] { new CategoriaResponse { IdCategoria = 1, Nombre = "Notebooks" } }),
                "/proveedores" => Json(new[] { new ProveedorResponse { IdProveedor = 1, RazonSocial = "Proveedor", Activo = true } }),
                _ => throw new InvalidOperationException($"Petición de prueba inesperada: {request.RequestUri.AbsolutePath}")
            };
        }

        private static HttpResponseMessage Json<T>(T value) => new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };
    }

    private sealed class TempDataMemoria : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class UrlHelperSimulado(ActionContext context) : IUrlHelper
    {
        public ActionContext ActionContext => context;
        public string? Action(UrlActionContext actionContext) => "/Productos";
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => url is not null && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\");
        public string? Link(string? routeName, object? values) => null;
        public string? RouteUrl(UrlRouteContext routeContext) => null;
    }

    private sealed class EntornoWeb : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "Frontend";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
