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

	public Task<HttpResponseMessage> CrearAsync(
		string nombre,
		string descripcion,
		decimal precio,
		int stock,
		int idCategoria,
		int idProveedor)
	{
		return CrearCliente().PostAsJsonAsync("/productos/", new
		{
			nombre,
			descripcion,
			precio,
			stock,
			idCategoria,
			idProveedor
		});
	}

	public Task<HttpResponseMessage> ActualizarAsync(
		int id,
		string nombre,
		string descripcion,
		decimal precio,
		int stock,
		int idCategoria,
		int idProveedor)
	{
		return CrearCliente().PutAsJsonAsync($"/productos/{id}", new
		{
			nombre,
			descripcion,
			precio,
			stock,
			idCategoria,
			idProveedor
		});
	}

	public Task<HttpResponseMessage> EliminarAsync(int id)
	{
		return CrearCliente().DeleteAsync($"/productos/{id}");
	}

	public async Task<List<ProductoResponse>> ObtenerTodosAsync()
	{
		var cliente = CrearCliente();

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(
			"/productos/");

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
		var ruta = string.IsNullOrWhiteSpace(texto)
			? "/productos/buscar"
			: $"/productos/buscar?texto={Uri.EscapeDataString(texto)}";

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(ruta);

		return productos ?? new List<ProductoResponse>();
	}

	public async Task<List<ProductoResponse>> ObtenerPorCategoriaAsync(int idCategoria)
	{
		var cliente = CrearCliente();

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(
			$"/productos/categoria/{idCategoria}");

		return productos ?? new List<ProductoResponse>();
	}

	public async Task<List<ProductoResponse>> ObtenerDisponiblesAsync()
	{
		var cliente = CrearCliente();

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(
			"/productos/disponibles");

		return productos ?? new List<ProductoResponse>();
	}
}
