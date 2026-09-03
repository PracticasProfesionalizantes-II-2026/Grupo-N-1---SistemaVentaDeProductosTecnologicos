using System.Net;
using System.Net.Http.Json;
using Frontend.Models.Api.Responses;

namespace Frontend.Services;

public class ProductosApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductosApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CrearCliente()
    {
        return _httpClientFactory.CreateClient("TotaltechApi");
    }

    public Task<HttpResponseMessage> CrearAsync(string n,string d,decimal p,int s,int c,int pr) => CrearCliente().PostAsJsonAsync("/productos/", new { nombre=n, descripcion=d, precio=p, stock=s, idCategoria=c, idProveedor=pr });
    public Task<HttpResponseMessage> ActualizarAsync(int id,string n,string d,decimal p,int s,int c,int pr) => CrearCliente().PutAsJsonAsync($"/productos/{id}", new { nombre=n, descripcion=d, precio=p, stock=s, idCategoria=c, idProveedor=pr });
    public Task<HttpResponseMessage> EliminarAsync(int id) => CrearCliente().DeleteAsync($"/productos/{id}");

    public async Task<List<ProductoResponse>> ObtenerTodosAsync()
    {
        var cliente = CrearCliente();

        var productos =
            await cliente.GetFromJsonAsync<List<ProductoResponse>>("/productos/");

        return productos ?? new List<ProductoResponse>();
    }

    public async Task<ProductoResponse?> ObtenerPorIdAsync(int id)
    {
        var cliente = CrearCliente();

        var respuesta = await cliente.GetAsync($"/productos/{id}");

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<ProductoResponse>();
    }

    public async Task<List<ProductoResponse>> BuscarAsync(string? texto)
    {
        var cliente = CrearCliente();

        var url = string.IsNullOrWhiteSpace(texto)
            ? "/productos/"
            : $"/productos/buscar?texto={Uri.EscapeDataString(texto)}";

        var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(url);

        return productos ?? new List<ProductoResponse>();
    }

    public async Task<List<ProductoResponse>> ObtenerPorCategoriaAsync(int id)
    {
        var cliente = CrearCliente();

        var productos =
            await cliente.GetFromJsonAsync<List<ProductoResponse>>(
                $"/productos/categoria/{id}");

        return productos ?? new List<ProductoResponse>();
    }

    public async Task<List<ProductoResponse>> ObtenerDisponiblesAsync()
    {
        var cliente = CrearCliente();

        var productos =
            await cliente.GetFromJsonAsync<List<ProductoResponse>>(
                "/productos/disponibles");

        return productos ?? new List<ProductoResponse>();
    }
}
