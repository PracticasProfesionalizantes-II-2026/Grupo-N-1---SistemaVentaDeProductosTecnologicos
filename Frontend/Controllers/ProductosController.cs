using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class ProductosController : Controller
{
    private readonly ProductosApiService _productosApiService;

    public ProductosController(ProductosApiService productosApiService)
    {
        _productosApiService = productosApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var productos = await _productosApiService.ObtenerTodosAsync();

        ViewData["TituloProductos"] = "Todos los productos";

        return View(productos);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var producto = await _productosApiService.ObtenerPorIdAsync(id);

        if (producto is null)
        {
            return NotFound();
        }

        return View(producto);
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(string? texto)
    {
        var productos = await _productosApiService.BuscarAsync(texto);

        ViewData["TituloProductos"] =
            string.IsNullOrWhiteSpace(texto)
                ? "Resultados de búsqueda"
                : $"Resultados para: {texto}";

        return View("Index", productos);
    }

    [HttpGet]
    public async Task<IActionResult> Categoria(int id)
    {
        var productos = await _productosApiService.ObtenerPorCategoriaAsync(id);

        ViewData["TituloProductos"] = $"Productos de la categoría {id}";

        return View("Index", productos);
    }

    [HttpGet]
    public async Task<IActionResult> Disponibles()
    {
        var productos = await _productosApiService.ObtenerDisponiblesAsync();

        ViewData["TituloProductos"] = "Productos disponibles";

        return View("Index", productos);
    }
}
