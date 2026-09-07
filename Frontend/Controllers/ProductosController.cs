// ============================================================================
// MÓDULO: PRODUCTOS
// RESPONSABILIDAD: Listar, consultar y gestionar productos desde MVC.
// ============================================================================
using Frontend.Models.Api.Requests;
using Frontend.Models.Api.Responses;
using Frontend.Models.ViewModels.Productos;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class ProductosController : Controller
{
    private readonly ProductosApiService _productosApiService;
    private readonly CategoriasApiService _categoriasApiService;

    public ProductosController(
        ProductosApiService productosApiService,
        CategoriasApiService categoriasApiService)
    {
        _productosApiService = productosApiService;
        _categoriasApiService = categoriasApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return await MostrarCatalogoAsync(
            _productosApiService.ObtenerTodosAsync(),
            "Todos los productos");
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
    public async Task<IActionResult> Crear()
    {
        await CargarCategoriasAsync();
        return View(new ProductoRequest());
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoRequest request)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync();
            return View(request);
        }

        var response = await _productosApiService.CrearAsync(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.IdCategoria, request.IdProveedor);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            await CargarCategoriasAsync();
            return View(request);
        }

        TempData["Mensaje"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _productosApiService.ObtenerPorIdAsync(id);
        if (producto is null) return NotFound();
        await CargarCategoriasAsync();
        return View(new ProductoRequest { Nombre = producto.Nombre, Descripcion = producto.Descripcion, Precio = producto.Precio, Stock = producto.Stock, IdCategoria = producto.IdCategoria, IdProveedor = producto.IdProveedor });
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProductoRequest request)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync();
            return View(request);
        }

        var response = await _productosApiService.ActualizarAsync(id, request.Nombre, request.Descripcion, request.Precio, request.Stock, request.IdCategoria, request.IdProveedor);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            await CargarCategoriasAsync();
            return View(request);
        }

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
        var titulo = string.IsNullOrWhiteSpace(texto)
            ? "Resultados de búsqueda"
            : $"Resultados para: {texto}";

        return await MostrarCatalogoAsync(
            _productosApiService.BuscarAsync(texto),
            titulo);
    }

    [HttpGet]
    public async Task<IActionResult> Categoria(int id)
    {
        var productosTask = _productosApiService.ObtenerPorCategoriaAsync(id);
        var categoriasTask = _categoriasApiService.ObtenerTodosAsync();
        await Task.WhenAll(productosTask, categoriasTask);

        var categorias = await categoriasTask;
        var categoria = categorias.FirstOrDefault(item => item.IdCategoria == id);
        if (categoria is null)
        {
            return NotFound();
        }

        ViewData["TituloProductos"] = $"Productos de {categoria.Nombre}";

        return View("Index", new CatalogoViewModel
        {
            Productos = await productosTask,
            Categorias = categorias,
            CategoriaSeleccionadaId = id
        });
    }

    [HttpGet]
    public async Task<IActionResult> Disponibles()
    {
        return await MostrarCatalogoAsync(
            _productosApiService.ObtenerDisponiblesAsync(),
            "Productos disponibles");
    }

    private async Task<IActionResult> MostrarCatalogoAsync(
        Task<List<ProductoResponse>> productosTask,
        string titulo)
    {
        var categoriasTask = _categoriasApiService.ObtenerTodosAsync();
        await Task.WhenAll(productosTask, categoriasTask);

        ViewData["TituloProductos"] = titulo;

        return View("Index", new CatalogoViewModel
        {
            Productos = await productosTask,
            Categorias = await categoriasTask
        });
    }

    private async Task CargarCategoriasAsync()
    {
        ViewBag.Categorias = await _categoriasApiService.ObtenerTodosAsync();
    }
}
