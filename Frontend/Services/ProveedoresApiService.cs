using System.Net;
using System.Net.Http.Json;
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;

namespace Frontend.Services;

public class ProveedoresApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProveedoresApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CrearCliente()
    {
        return _httpClientFactory.CreateClient("TotaltechApi");
    }

    public Task<HttpResponseMessage> CrearAsync(ProveedorRequest request) => CrearCliente().PostAsJsonAsync("/proveedores/", request);

    public Task<HttpResponseMessage> ActualizarAsync(int id, ProveedorRequest request) => CrearCliente().PutAsJsonAsync($"/proveedores/{id}", request);

    public Task<HttpResponseMessage> EliminarAsync(int id) => CrearCliente().DeleteAsync($"/proveedores/{id}");

    public async Task<List<ProveedorResponse>> ObtenerTodosAsync()
    {
        var cliente = CrearCliente();

        var proveedores =
            await cliente.GetFromJsonAsync<List<ProveedorResponse>>(
                "/proveedores/");

        return proveedores ?? new List<ProveedorResponse>();
    }

    public async Task<ProveedorResponse?> ObtenerPorIdAsync(int id)
    {
        var cliente = CrearCliente();

        var respuesta =
            await cliente.GetAsync($"/proveedores/{id}");

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<ProveedorResponse>();
    }
}
