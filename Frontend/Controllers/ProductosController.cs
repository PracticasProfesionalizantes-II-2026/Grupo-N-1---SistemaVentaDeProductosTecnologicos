// ============================================================================
// MÓDULO: PRODUCTOS
// RESPONSABILIDAD: Listar, consultar y gestionar productos desde MVC.
// ============================================================================
using Frontend.Models.Api.Requests;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet, Authorize(Roles = "Admin")]
    public IActionResult Crear() => View(new ProductoRequest());

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var response = await _productosApiService.CrearAsync(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.IdCategoria, request.IdProveedor);
        if (!response.IsSuccessStatusCode) { ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync()); return View(request); }
        TempData["Mensaje"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _productosApiService.ObtenerPorIdAsync(id);
        if (producto is null) return NotFound();
        return View(new ProductoRequest { Nombre = producto.Nombre, Descripcion = producto.Descripcion, Precio = producto.Precio, Stock = producto.Stock, IdCategoria = producto.IdCategoria, IdProveedor = producto.IdProveedor });
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProductoRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var response = await _productosApiService.ActualizarAsync(id, request.Nombre, request.Descripcion, request.Precio, request.Stock, request.IdCategoria, request.IdProveedor);
        if (!response.IsSuccessStatusCode) { ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync()); return View(request); }
        TempData["Mensaje"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var response = await _productosApiService.EliminarAsync(id);
        TempData[response.IsSuccessStatusCode ? "Mensaje" : "Error"] = response.IsSuccessStatusCode ? "Producto eliminado correctamente." : "No se pudo eliminar el producto.";
        return RedirectToAction(nameof(Index));
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
