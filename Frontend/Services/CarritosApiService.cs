// Estructura reservada. Implementación pendiente.
using System.Net;
using System.Net.Http.Json;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;

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
