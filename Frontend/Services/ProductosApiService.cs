using System.Net;
using System.Net.Http.Json;
using Frontend.Models.Api.Responses;

namespace Frontend.Services;

public class ProductosApiService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ProductoImagenResolver _imagenResolver;

	public ProductosApiService(
		IHttpClientFactory httpClientFactory,
		ProductoImagenResolver imagenResolver)
	{
		_httpClientFactory = httpClientFactory;
		_imagenResolver = imagenResolver;
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

		return AplicarImagenes(productos);
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

		return AplicarImagen(
			await respuesta.Content.ReadFromJsonAsync<ProductoResponse>());
	}

	public async Task<List<ProductoResponse>> BuscarAsync(string? texto)
	{
		var cliente = CrearCliente();
		var ruta = string.IsNullOrWhiteSpace(texto)
			? "/productos/buscar"
			: $"/productos/buscar?texto={Uri.EscapeDataString(texto)}";

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(ruta);

		return AplicarImagenes(productos);
	}

	public async Task<List<ProductoResponse>> ObtenerPorCategoriaAsync(int idCategoria)
	{
		var cliente = CrearCliente();

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(
			$"/productos/categoria/{idCategoria}");

		return AplicarImagenes(productos);
	}

	public async Task<List<ProductoResponse>> ObtenerDisponiblesAsync()
	{
		var cliente = CrearCliente();

		var productos = await cliente.GetFromJsonAsync<List<ProductoResponse>>(
			"/productos/disponibles");

		return AplicarImagenes(productos);
	}

	public async Task<(CatalogoProductosResponse? Catalogo, string? Error)> ObtenerCatalogoAsync(
		string? texto, int? idCategoria, decimal? precioMin, decimal? precioMax,
		bool soloDisponibles, int pagina, int tamanoPagina)
	{
		var parametros = new Dictionary<string, string?>
		{
			["texto"] = texto,
			["idCategoria"] = idCategoria?.ToString(),
			["precioMin"] = precioMin?.ToString(System.Globalization.CultureInfo.InvariantCulture),
			["precioMax"] = precioMax?.ToString(System.Globalization.CultureInfo.InvariantCulture),
			["soloDisponibles"] = soloDisponibles.ToString().ToLowerInvariant(),
			["pagina"] = pagina.ToString(),
			["tamanoPagina"] = tamanoPagina.ToString()
		};
		var query = string.Join("&", parametros
			.Where(item => !string.IsNullOrWhiteSpace(item.Value))
			.Select(item => $"{item.Key}={Uri.EscapeDataString(item.Value!)}"));
		var respuesta = await CrearCliente().GetAsync($"/productos/catalogo?{query}");
		if (respuesta.StatusCode == HttpStatusCode.BadRequest)
		{
			var error = await respuesta.Content.ReadFromJsonAsync<ErrorCatalogoResponse>();
			return (null, error?.Mensaje ?? "Los filtros ingresados no son válidos.");
		}
		respuesta.EnsureSuccessStatusCode();
		var catalogo = await respuesta.Content.ReadFromJsonAsync<CatalogoProductosResponse>();
		if (catalogo is not null) AplicarImagenes(catalogo.Items);
		return (catalogo, null);
	}

	private List<ProductoResponse> AplicarImagenes(List<ProductoResponse>? productos)
	{
		var resultado = productos ?? [];
		foreach (var producto in resultado)
		{
			AplicarImagen(producto);
		}

		return resultado;
	}

	private ProductoResponse? AplicarImagen(ProductoResponse? producto)
	{
		if (producto is not null)
		{
			producto.ImagenUrl = _imagenResolver.Resolver(producto.Nombre);
		}

		return producto;
	}
}
