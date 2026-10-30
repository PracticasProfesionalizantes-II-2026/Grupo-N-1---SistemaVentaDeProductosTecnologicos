using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Frontend.Controllers;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Totaltech.IntegrationTests.Frontend;

public sealed class ProductoImportacionHttpTests
{
    [Theory]
    [InlineData(false)][InlineData(true)]
    public async Task SoloAdminAccedeALasCuatroAcciones(bool cliente)
    {
        using var api = new ImportacionApiPrueba { Admin = !cliente };
        await using var factory = ImportacionHttp.Factory(api);
        using var http = ImportacionHttp.Cliente(factory);
        if (cliente) await ImportacionHttp.LoginAsync(http);
        foreach (var accion in new[] { "Importar", "PlantillaImportacionCsv" })
        {
            using var response = await http.GetAsync("/Productos/" + accion);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Home/Login", response.Headers.Location!.OriginalString);
        }
        foreach (var accion in new[] { "PrevisualizarImportacion", "ConfirmarImportacion" })
        {
            using var response = await http.PostAsync("/Productos/" + accion, new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
        Assert.Equal(0, api.Importaciones);
        using var catalogo = await http.GetAsync("/Productos");
        Assert.Equal(HttpStatusCode.OK, catalogo.StatusCode);
        Assert.DoesNotContain("Importar catálogo", WebUtility.HtmlDecode(await catalogo.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task AdminVeAccesosYAmbosPostRechazanAusenciaDeAntiforgery()
    {
        using var api = new ImportacionApiPrueba();
        await using var factory = ImportacionHttp.Factory(api);
        using var http = ImportacionHttp.Cliente(factory);
        await ImportacionHttp.LoginAsync(http);
        foreach (var pagina in new[] { "/Administracion", "/Productos", "/Productos/Importar" })
            Assert.Contains("Importar catálogo", WebUtility.HtmlDecode(await http.GetStringAsync(pagina)));
        foreach (var accion in new[] { "PrevisualizarImportacion", "ConfirmarImportacion" })
        {
            using var response = await http.PostAsync("/Productos/" + accion, new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.Equal(0, api.Importaciones);
    }

    [Fact]
    public async Task UploadRevisionValidacionYConfirmacionFuncionanConApiSimulada()
    {
        using var api = new ImportacionApiPrueba();
        await using var factory = ImportacionHttp.Factory(api);
        using var http = ImportacionHttp.Cliente(factory);
        await ImportacionHttp.LoginAsync(http);
        var html = await http.GetStringAsync("/Productos/Importar");
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(ImportacionHttp.Token(html)), "__RequestVerificationToken");
        multipart.Add(new StringContent("1"), "IdProveedor");
        multipart.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("Nombre;Precio;Stock;CategoriaNombre\nNotebook;1234,56;5;Notebooks")), "Archivo", "catalogo.csv");
        using var preview = await http.PostAsync("/Productos/PrevisualizarImportacion", multipart);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        html = await preview.Content.ReadAsStringAsync();
        Assert.Contains("Lote validado", html);
        Assert.DoesNotContain("disabled", ImportacionHttp.BotonConfirmar(html));
        var operacion = ImportacionHttp.Valor(html, "IdOperacion");
        var campos = ImportacionHttp.Campos(html, operacion);
        using var resultado = await http.PostAsync("/Productos/ConfirmarImportacion", new FormUrlEncodedContent(campos));
        Assert.Equal(HttpStatusCode.Redirect, resultado.StatusCode);
        Assert.Equal("/Productos", resultado.Headers.Location!.OriginalString);
        Assert.Equal(1, api.Confirmaciones);
        Assert.Equal(Guid.Parse(operacion), api.UltimaOperacion);
        var catalogo = WebUtility.HtmlDecode(await http.GetStringAsync("/Productos"));
        Assert.Contains("Se importaron 1 productos correctamente.", catalogo);
    }

    [Fact]
    public async Task ApiAusenteConservaExtraccionYBotonDeshabilitado()
    {
        using var api = new ImportacionApiPrueba { BackendAusente = true };
        await using var factory = ImportacionHttp.Factory(api);
        using var http = ImportacionHttp.Cliente(factory);
        await ImportacionHttp.LoginAsync(http);
        var html = await http.GetStringAsync("/Productos/Importar");
        using var response = await http.PostAsync("/Productos/PrevisualizarImportacion", new FormUrlEncodedContent(ImportacionHttp.Campos(html, Guid.NewGuid().ToString())));
        html = await response.Content.ReadAsStringAsync();
        Assert.Contains("todav&#xED;a no est&#xE1; disponible en Backend", html);
        Assert.Contains("Notebook", html);
        Assert.Contains("disabled", ImportacionHttp.BotonConfirmar(html));
        Assert.Equal(0, api.Confirmaciones);
    }

    [Fact]
    public async Task MilFilasSeEnlazanCompletasSinLimitePredeterminadoDeFormularios()
    {
        using var api = new ImportacionApiPrueba();
        await using var factory = ImportacionHttp.Factory(api);
        using var http = ImportacionHttp.Cliente(factory);
        await ImportacionHttp.LoginAsync(http);
        var html = await http.GetStringAsync("/Productos/Importar");
        var campos = ImportacionHttp.Campos(html, Guid.NewGuid().ToString()).Where(c => !c.Key.StartsWith("Filas[")).ToList();
        for (var i = 0; i < 1000; i++)
        {
            campos.Add(new($"Filas[{i}].Nombre", $"Notebook {i}"));
            campos.Add(new($"Filas[{i}].Descripcion", ""));
            campos.Add(new($"Filas[{i}].Precio", "1.00"));
            campos.Add(new($"Filas[{i}].Stock", "0"));
            campos.Add(new($"Filas[{i}].IdCategoria", "1"));
        }
        using var response = await http.PostAsync("/Productos/PrevisualizarImportacion", new FormUrlEncodedContent(campos));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1000, api.UltimasFilas);
        Assert.Contains("Filas[999].Nombre", await response.Content.ReadAsStringAsync());
    }
}

internal static class ImportacionHttp
{
    internal static WebApplicationFactory<HomeController> Factory(ImportacionApiPrueba api) => new WebApplicationFactory<HomeController>()
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseStaticWebAssets();
            builder.ConfigureServices(services => services.AddSingleton<IHttpClientFactory>(api));
        });
    internal static HttpClient Cliente(WebApplicationFactory<HomeController> factory) => factory.CreateClient(new()
        { BaseAddress = new Uri("https://frontend.test"), AllowAutoRedirect = false });
    internal static async Task LoginAsync(HttpClient http)
    {
        var html = await http.GetStringAsync("/Home/Login");
        using var response = await http.PostAsync("/Home/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            { ["__RequestVerificationToken"] = Token(html), ["Email"] = "admin@test.local", ["Contrasena"] = "ClavePrueba123" }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
    internal static string Token(string html) => Valor(html, "__RequestVerificationToken");
    internal static string Valor(string html, string nombre)
    {
        var match = Regex.Match(html, "name=\"" + Regex.Escape(nombre) + "\"[^>]*value=\"([^\"]*)\"");
        Assert.True(match.Success, "No se encontró el campo " + nombre);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
    internal static string BotonConfirmar(string html) => Regex.Match(html, "<button[^>]*data-importacion-confirmar[^>]*>").Value;
    internal static List<KeyValuePair<string, string>> Campos(string html, string operacion) => new Dictionary<string, string>
    {
        ["__RequestVerificationToken"] = Token(html), ["IdProveedor"] = "1", ["IdOperacion"] = operacion,
        ["Filas[0].Nombre"] = "Notebook", ["Filas[0].Descripcion"] = "", ["Filas[0].Precio"] = "1234.56",
        ["Filas[0].Stock"] = "5", ["Filas[0].CategoriaNombre"] = "Notebooks", ["Filas[0].IdCategoria"] = "1"
    }.ToList();
}

internal sealed class ImportacionApiPrueba : HttpMessageHandler, IHttpClientFactory
{
    public bool Admin { get; init; } = true;
    public bool BackendAusente { get; init; }
    public int Importaciones { get; private set; }
    public int Confirmaciones { get; private set; }
    public int UltimasFilas { get; private set; }
    public Guid UltimaOperacion { get; private set; }
    public HttpClient CreateClient(string name) => new(this, false) { BaseAddress = new Uri("https://api.test/") };
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var ruta = request.RequestUri!.AbsolutePath.TrimEnd('/');
        if (ruta.StartsWith("/productos/importacion/"))
        {
            Importaciones++;
            if (BackendAusente) return new(HttpStatusCode.NotFound);
            var dto = await request.Content!.ReadFromJsonAsync<ProductoImportacionConfirmarRequest>(ct);
            UltimasFilas = dto!.Filas.Count;
            if (ruta.EndsWith("confirmar"))
            {
                Confirmaciones++;
                UltimaOperacion = dto.IdOperacion;
                return Json(new ProductoImportacionConfirmacionResponse { IdOperacion = dto.IdOperacion, Importados = dto.Filas.Count });
            }
            return Json(new ProductoImportacionPrevisualizacionResponse { Filas = dto.Filas, PuedeConfirmar = true });
        }
        return ruta switch
        {
            "/auth/login" => Json(new { IdUsuario = 7, Nombre = "Usuario", Apellido = "Prueba", Email = "admin@test.local", Rol = Admin ? 1 : 0,
                AccessToken = "token-ficticio", ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1) }),
            "/usuarios/7" => Json(new UsuarioResponse { IdUsuario = 7, Nombre = "Usuario", Apellido = "Prueba", Email = "admin@test.local", Rol = Admin ? 1 : 0, Activo = true }),
            "/productos/catalogo" => Json(new CatalogoProductosResponse()),
            "/productos" => Json(Array.Empty<ProductoResponse>()),
            "/usuarios" => Json(Array.Empty<UsuarioResumenResponse>()),
            "/pedidos/estado/Pendiente" => Json(Array.Empty<PedidoResumenResponse>()),
            "/categorias" => Json(new[] { new CategoriaResponse { IdCategoria = 1, Nombre = "Notebooks" } }),
            "/proveedores" => Json(new[] { new ProveedorResponse { IdProveedor = 1, RazonSocial = "Proveedor activo", Activo = true } }),
            "/carritos/actual" => Json(new CarritoResumenResponse()),
            _ => throw new InvalidOperationException("Ruta no prevista por el test: " + ruta)
        };
    }
    private static HttpResponseMessage Json<T>(T contenido) => new(HttpStatusCode.OK) { Content = JsonContent.Create(contenido) };
}
