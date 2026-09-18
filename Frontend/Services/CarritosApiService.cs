// ============================================================================
// MÓDULO ACTUAL: SERVICIO API DE CATEGORÍAS
// RESPONSABILIDAD: Consultar y mantener categorías mediante el cliente TotaltechApi.
// DEUDA TÉCNICA: El archivo se llama CarritosApiService, pero contiene CategoriasApiService.
// ============================================================================
using System.Net;
using System.Net.Http.Json;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Frontend.Services.Interfaces;

namespace Frontend.Services;

public class CategoriasApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoriasApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CrearCliente()
    {
        return _httpClientFactory.CreateClient("TotaltechApi");
    }

    public async Task<List<CategoriaResponse>> ObtenerTodosAsync()
    {
        var cliente = CrearCliente();

        var categorias = await cliente.GetFromJsonAsync<List<CategoriaResponse>>(
            "/categorias/");

        return categorias ?? new List<CategoriaResponse>();
    }

    public async Task<CategoriaResponse?> ObtenerPorIdAsync(int id)
    {
        var cliente = CrearCliente();

        var respuesta = await cliente.GetAsync($"/categorias/{id}");

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<CategoriaResponse>();
    }

    public async Task<(bool Exitoso, string? Error)> CrearAsync(
        CategoriaRequest request)
    {
        var cliente = CrearCliente();

        var respuesta = await cliente.PostAsJsonAsync(
            "/categorias/",
            request);

        if (respuesta.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await respuesta.Content.ReadAsStringAsync();

        return (false, error);
    }

    public async Task<(bool Exitoso, string? Error)> ActualizarAsync(
        int id,
        CategoriaRequest request)
    {
        var cliente = CrearCliente();

        var respuesta = await cliente.PutAsJsonAsync(
            $"/categorias/{id}",
            request);

        if (respuesta.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await respuesta.Content.ReadAsStringAsync();

        return (false, error);
    }

    public async Task<(bool Exitoso, HttpStatusCode Estado, string? Error)> EliminarAsync(
        int id)
    {
        var cliente = CrearCliente();

        var respuesta = await cliente.DeleteAsync($"/categorias/{id}");

        if (respuesta.IsSuccessStatusCode)
        {
            return (true, respuesta.StatusCode, null);
        }

        var error = await respuesta.Content.ReadAsStringAsync();

        return (false, respuesta.StatusCode, error);
    }
}

public sealed class CarritosApiService : ICarritosApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProductoImagenResolver _imagenResolver;

    public CarritosApiService(IHttpClientFactory httpClientFactory, ProductoImagenResolver imagenResolver)
    {
        _httpClientFactory = httpClientFactory;
        _imagenResolver = imagenResolver;
    }

    public async Task<CarritoResumenResponse?> ObtenerActualAsync(CancellationToken cancellationToken = default)
    {
        var respuesta = await Cliente().GetAsync("/carritos/actual", cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return null;
        respuesta.EnsureSuccessStatusCode();
        return AplicarImagenes(await respuesta.Content.ReadFromJsonAsync<CarritoResumenResponse>(cancellationToken));
    }

    public Task<CarritoApiResultado> AgregarAsync(int idProducto, int cantidad, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Post, "/carritos/actual/productos", new { idProducto, cantidad }, cancellationToken);

    public Task<CarritoApiResultado> ActualizarCantidadAsync(int idProducto, int cantidad, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Patch, $"/carritos/actual/productos/{idProducto}", new { cantidad }, cancellationToken);

    public Task<CarritoApiResultado> EliminarAsync(int idProducto, CancellationToken cancellationToken = default) =>
        EnviarAsync(HttpMethod.Delete, $"/carritos/actual/productos/{idProducto}", null, cancellationToken);

    private async Task<CarritoApiResultado> EnviarAsync(HttpMethod metodo, string ruta, object? cuerpo,
        CancellationToken cancellationToken)
    {
        using var solicitud = new HttpRequestMessage(metodo, ruta);
        if (cuerpo is not null) solicitud.Content = JsonContent.Create(cuerpo);
        using var respuesta = await Cliente().SendAsync(solicitud, cancellationToken);
        if (!respuesta.IsSuccessStatusCode)
            return new(null, respuesta.StatusCode, await LeerErrorAsync(respuesta, cancellationToken));
        return new(AplicarImagenes(await respuesta.Content.ReadFromJsonAsync<CarritoResumenResponse>(cancellationToken)), respuesta.StatusCode, null);
    }

    private HttpClient Cliente() => _httpClientFactory.CreateClient("TotaltechApi");

    private CarritoResumenResponse? AplicarImagenes(CarritoResumenResponse? resumen)
    {
        if (resumen is not null)
            foreach (var item in resumen.Items) item.ImagenUrl = _imagenResolver.Resolver(item.Nombre);
        return resumen;
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        var error = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(error) ? "No pudimos actualizar el carrito." : error.Trim('"');
    }
}
