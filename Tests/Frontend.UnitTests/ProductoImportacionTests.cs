using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Frontend.Controllers;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace Frontend.UnitTests;

public sealed class ProductoImportacionTests
{
    [Fact]
    public async Task PreviewExtraeResuelveCategoriaYUsaContratoCamelCase()
    {
        using var api = new Api();
        var controller = Controller(api);
        var modelo = Modelo();
        modelo.Archivo = Csv("Nombre;Precio;Stock;Categoria\nNotebook;1234,56;5;NÓTEBOOKS");
        var view = Assert.IsType<ViewResult>(await controller.PrevisualizarImportacion(modelo));
        Assert.Equal("Importar", view.ViewName);
        Assert.True(modelo.PuedeConfirmar);
        Assert.Equal(1, modelo.Filas[0].IdCategoria);
        using var json = JsonDocument.Parse(Assert.Single(api.Cuerpos));
        Assert.Equal(1, json.RootElement.GetProperty("idProveedor").GetInt32());
        var fila = json.RootElement.GetProperty("filas")[0];
        Assert.Equal(1234.56m, fila.GetProperty("precio").GetDecimal());
        Assert.Equal(1, fila.GetProperty("numeroFila").GetInt32());
        Assert.False(fila.TryGetProperty("origen", out _));
        Assert.Equal(["TotaltechApi", "TotaltechApi", "TotaltechApi"], api.Clientes);
    }

    [Fact]
    public async Task PreviewAplicaNormalizacionApiYLimpiaModelStateDeLosCampos()
    {
        using var api = new Api { Normalizar = true };
        var controller = Controller(api);
        controller.ModelState.SetModelValue("Filas[0].Nombre", "Notebook", "Notebook");
        controller.ModelState["Filas[0].Nombre"]!.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
        var modelo = Modelo();
        await controller.PrevisualizarImportacion(modelo);
        Assert.Equal("Notebook normalizada", modelo.Filas[0].Nombre);
        Assert.False(controller.ModelState.ContainsKey("Filas[0].Nombre"));
        Assert.True(modelo.PuedeConfirmar);
    }

    [Theory]
    [InlineData(404, "Backend")][InlineData(405, "Backend")]
    [InlineData(401, "sesión")][InlineData(403, "sesión")]
    [InlineData(409, "conflicto")][InlineData(500, "API")]
    public async Task PreviewRechazoConservaFilasYNoHabilitaConfirmacion(int estado, string mensaje)
    {
        using var api = new Api { PreviewStatus = (HttpStatusCode)estado };
        var controller = Controller(api);
        var modelo = Modelo();
        await controller.PrevisualizarImportacion(modelo);
        Assert.False(modelo.PuedeConfirmar);
        Assert.Equal("Notebook", Assert.Single(modelo.Filas).Nombre);
        Assert.Contains(Errores(controller), e => e.Contains(mensaje));
        Assert.Single(api.Cuerpos);
    }

    [Theory]
    [InlineData("red")][InlineData("timeout")][InlineData("json")][InlineData("incompleta")]
    public async Task PreviewResilienteConservaBorrador(string fallo)
    {
        using var api = new Api { Fallo = fallo };
        var controller = Controller(api);
        var modelo = Modelo();
        var operacion = modelo.IdOperacion;
        await controller.PrevisualizarImportacion(modelo);
        Assert.False(modelo.PuedeConfirmar);
        Assert.Equal(operacion, modelo.IdOperacion);
        Assert.Single(modelo.Filas);
        Assert.NotEmpty(Errores(controller));
    }

    [Fact]
    public async Task PreviewErroresPorFilaSeAsocianAlCampoYNoConfirman()
    {
        using var api = new Api { PreviewStatus = HttpStatusCode.UnprocessableEntity };
        var controller = Controller(api);
        var modelo = Modelo();
        await controller.PrevisualizarImportacion(modelo);
        Assert.False(modelo.PuedeConfirmar);
        Assert.Contains(controller.ModelState["Filas[0].Nombre"]!.Errors, e => e.ErrorMessage.Contains("ya existe"));
    }

    [Fact]
    public async Task DatosInvalidosNoSeEnvianYConservanTextoOriginal()
    {
        using var api = new Api();
        var controller = Controller(api);
        var modelo = Modelo();
        modelo.Filas[0].Precio = "1.234";
        modelo.Filas[0].Stock = "demasiado";
        modelo.Filas.Add(new() { Nombre = "  NÓTEBOOK ", Precio = "10", Stock = "1", IdCategoria = 1 });
        await controller.PrevisualizarImportacion(modelo);
        Assert.Empty(api.Cuerpos);
        Assert.Equal("1.234", modelo.Filas[0].Precio);
        Assert.Contains(Errores(controller), e => e.Contains("repetido"));
        Assert.True(controller.ModelState.ContainsKey("Filas[0].Precio"));
        Assert.True(controller.ModelState.ContainsKey("Filas[0].Stock"));
    }

    [Theory]
    [InlineData(2, 1)][InlineData(1, 999)]
    public async Task ProveedorInactivoOCategoriaInexistenteBloqueaEnvio(int proveedor, int categoria)
    {
        using var api = new Api();
        var controller = Controller(api);
        var modelo = Modelo();
        modelo.IdProveedor = proveedor;
        modelo.Filas[0].IdCategoria = categoria;
        await controller.ConfirmarImportacion(modelo);
        Assert.Empty(api.Cuerpos);
        Assert.NotEmpty(Errores(controller));
    }

    [Fact]
    public async Task CategoriaAmbiguaNoSeAsignaAutomaticamente()
    {
        using var api = new Api { CategoriaAmbigua = true };
        var controller = Controller(api);
        var modelo = Modelo();
        modelo.Filas[0].IdCategoria = null;
        await controller.PrevisualizarImportacion(modelo);
        Assert.Null(modelo.Filas[0].IdCategoria);
        Assert.Empty(api.Cuerpos);
    }

    [Fact]
    public async Task ConfirmacionExitosaRedirigeConCantidadYConservaIdOperacion()
    {
        using var api = new Api();
        var controller = Controller(api);
        var modelo = Modelo();
        var redirect = Assert.IsType<RedirectToActionResult>(await controller.ConfirmarImportacion(modelo));
        Assert.Equal("Index", redirect.ActionName);
        Assert.Contains("1 productos", controller.TempData["Mensaje"]!.ToString());
        using var json = JsonDocument.Parse(Assert.Single(api.Cuerpos));
        Assert.Equal(modelo.IdOperacion, json.RootElement.GetProperty("idOperacion").GetGuid());
        Assert.Single(api.Cuerpos);
    }

    [Theory]
    [InlineData("red")][InlineData("timeout")][InlineData("json")][InlineData("incompleta")]
    public async Task ConfirmacionInciertaNoReintentaNiAfirmaExito(string fallo)
    {
        using var api = new Api { Fallo = fallo };
        var controller = Controller(api);
        var modelo = Modelo();
        var operacion = modelo.IdOperacion;
        Assert.IsType<ViewResult>(await controller.ConfirmarImportacion(modelo));
        Assert.Equal(operacion, modelo.IdOperacion);
        Assert.False(modelo.PuedeConfirmar);
        Assert.Null(controller.TempData["Mensaje"]);
        Assert.Contains(Errores(controller), e => e.Contains("Revisá el catálogo"));
        Assert.Single(api.Cuerpos);
    }

    [Theory]
    [InlineData(404)][InlineData(405)][InlineData(409)][InlineData(422)][InlineData(500)]
    public async Task ConfirmacionRechazadaNoRedirigeNiPierdeFilas(int estado)
    {
        using var api = new Api { ConfirmStatus = (HttpStatusCode)estado };
        var controller = Controller(api);
        var modelo = Modelo();
        Assert.IsType<ViewResult>(await controller.ConfirmarImportacion(modelo));
        Assert.Single(modelo.Filas);
        Assert.NotEmpty(Errores(controller));
        Assert.Null(controller.TempData["Mensaje"]);
    }

    [Fact]
    public async Task OpcionesApiCaidasPermitenExtraccionLocalSinConfirmar()
    {
        using var api = new Api { OpcionesCaidas = true };
        var controller = Controller(api);
        var modelo = new ProductoImportacionViewModel { Archivo = Csv("Nombre;Precio;Stock\nNotebook;10;5") };
        await controller.PrevisualizarImportacion(modelo);
        Assert.Single(modelo.Filas);
        Assert.False(modelo.PuedeConfirmar);
        Assert.Empty(api.Cuerpos);
        Assert.Contains(Errores(controller), e => e.Contains("falta validación Backend"));
    }

    [Fact]
    public async Task CancelacionDelRequestSePropaga()
    {
        using var api = new Api();
        var controller = Controller(api);
        controller.HttpContext.RequestAborted = new CancellationToken(true);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => controller.PrevisualizarImportacion(Modelo()));
    }

    [Fact]
    public async Task NoInventaIdDeOperacionSiFaltaEnElFormulario()
    {
        using var api = new Api();
        var controller = Controller(api);
        var modelo = Modelo();
        modelo.IdOperacion = Guid.Empty;
        await controller.ConfirmarImportacion(modelo);
        Assert.Empty(api.Cuerpos);
        Assert.Contains(Errores(controller), e => e.Contains("operación no es válida"));
    }

    [Fact]
    public async Task PlantillaTieneBomYSePuedeExtraerSinPerderPrecio()
    {
        using var api = new Api();
        var file = Assert.IsType<FileContentResult>(Controller(api).PlantillaImportacionCsv());
        Assert.Equal("plantilla-catalogo.csv", file.FileDownloadName);
        Assert.Equal(new byte[] { 0xef, 0xbb, 0xbf }, file.FileContents.Take(3));
        var resultado = await new CatalogoArchivoExtractor(new CatalogoExtractorHeuristico()).ExtraerAsync(
            new FormFile(new MemoryStream(file.FileContents), 0, file.FileContents.Length, "Archivo", "plantilla.csv"));
        Assert.Empty(resultado.Errores);
        Assert.Equal("1234,56", Assert.Single(resultado.Filas).Precio);
    }

    private static ProductoImportacionViewModel Modelo() => new() { IdProveedor = 1, IdOperacion = Guid.NewGuid(),
        Filas = [new() { Nombre = "Notebook", Precio = "1234.56", Stock = "5", CategoriaNombre = "Notebooks", IdCategoria = 1 }] };
    private static IFormFile Csv(string texto)
    {
        var bytes = Encoding.UTF8.GetBytes(texto);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Archivo", "catalogo.csv");
    }
    private static IEnumerable<string> Errores(ProductosController controller) => controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
    private static ProductosController Controller(Api api)
    {
        var controller = new ProductosController(new(api, new()), new(api), new(api), new(new Entorno()),
            NullLogger<ProductosController>.Instance, new(new CatalogoExtractorHeuristico()));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.TempData = new TempDataDictionary(controller.HttpContext, new TempDataMemoria());
        return controller;
    }

    private sealed class Entorno : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Frontend";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
    private sealed class TempDataMemoria : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
    private sealed class Api : HttpMessageHandler, IHttpClientFactory
    {
        public List<string> Clientes { get; } = [];
        public List<string> Cuerpos { get; } = [];
        public HttpStatusCode PreviewStatus { get; init; } = HttpStatusCode.OK;
        public HttpStatusCode ConfirmStatus { get; init; } = HttpStatusCode.OK;
        public string? Fallo { get; init; }
        public bool OpcionesCaidas { get; init; }
        public bool CategoriaAmbigua { get; init; }
        public bool Normalizar { get; init; }
        public HttpClient CreateClient(string name)
        { Clientes.Add(name); return new(this, false) { BaseAddress = new Uri("https://api.test/") }; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var ruta = request.RequestUri!.AbsolutePath.TrimEnd('/');
            if (ruta is "/categorias" or "/proveedores")
            {
                if (OpcionesCaidas) throw new HttpRequestException();
                if (ruta == "/categorias") return Json(HttpStatusCode.OK, CategoriaAmbigua
                    ? new[] { new CategoriaResponse { IdCategoria = 1, Nombre = "Notebooks" }, new CategoriaResponse { IdCategoria = 2, Nombre = "NOTEBOOKS" } }
                    : [new CategoriaResponse { IdCategoria = 1, Nombre = "Notebooks" }]);
                return Json(HttpStatusCode.OK, new[] { new ProveedorResponse { IdProveedor = 1, RazonSocial = "Activo", Activo = true },
                    new ProveedorResponse { IdProveedor = 2, RazonSocial = "Inactivo", Activo = false } });
            }
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Contains(ruta, new[] { "/productos/importacion/previsualizar", "/productos/importacion/confirmar" });
            var body = await request.Content!.ReadAsStringAsync(ct);
            Cuerpos.Add(body);
            if (Fallo == "red") throw new HttpRequestException("detalle interno");
            if (Fallo == "timeout") throw new TaskCanceledException("detalle interno");
            if (Fallo == "json") return new(HttpStatusCode.OK) { Content = new StringContent("no-json") };
            if (Fallo == "incompleta") return Json(HttpStatusCode.OK, new { });
            var confirmar = ruta.EndsWith("confirmar");
            var estado = confirmar ? ConfirmStatus : PreviewStatus;
            var dto = JsonSerializer.Deserialize<ProductoImportacionConfirmarRequest>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            if (estado == HttpStatusCode.UnprocessableEntity) return Json(estado, new ProductoImportacionPrevisualizacionResponse {
                Filas = dto.Filas, Errores = [new() { NumeroFila = 1, Campo = "nombre", Mensaje = "El nombre ya existe." }] });
            if (Normalizar) dto.Filas[0].Nombre = "Notebook normalizada";
            return confirmar ? Json(estado, new ProductoImportacionConfirmacionResponse { IdOperacion = dto.IdOperacion, Importados = dto.Filas.Count })
                : Json(estado, new ProductoImportacionPrevisualizacionResponse { Filas = dto.Filas, PuedeConfirmar = true });
        }
        private static HttpResponseMessage Json<T>(HttpStatusCode status, T contenido) => new(status) { Content = JsonContent.Create(contenido) };
    }
}
