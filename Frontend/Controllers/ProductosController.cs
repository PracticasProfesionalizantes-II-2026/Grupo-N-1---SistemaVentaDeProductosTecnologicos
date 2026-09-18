// ============================================================================
// MÓDULO: PRODUCTOS
// RESPONSABILIDAD: Coordinar catálogo, detalle y mantenimiento de productos.
// INTEGRACIÓN: Delega productos y categorías en sus servicios de API respectivos.
// ACCESO: El catálogo es público; crear, editar y eliminar requieren rol Admin.
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
    private readonly ProveedoresApiService _proveedoresApiService;

    public ProductosController(
        ProductosApiService productosApiService,
        CategoriasApiService categoriasApiService,
        ProveedoresApiService proveedoresApiService)
    {
        _productosApiService = productosApiService;
        _categoriasApiService = categoriasApiService;
        _proveedoresApiService = proveedoresApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? texto, int? idCategoria, decimal? precioMin,
        decimal? precioMax, bool soloDisponibles = false, int pagina = 1, int tamanoPagina = 12)
    {
        var modelo = new CatalogoViewModel { Texto = texto, CategoriaSeleccionadaId = idCategoria,
            PrecioMin = precioMin, PrecioMax = precioMax, SoloDisponibles = soloDisponibles,
            Pagina = pagina, TamanoPagina = tamanoPagina };
        try
        {
            var categoriasTask = _categoriasApiService.ObtenerTodosAsync();
            var catalogoTask = _productosApiService.ObtenerCatalogoAsync(texto, idCategoria, precioMin, precioMax,
                soloDisponibles, pagina, tamanoPagina);
            await Task.WhenAll(categoriasTask, catalogoTask);
            modelo.Categorias = await categoriasTask;
            var (catalogo, error) = await catalogoTask;
            modelo.Error = error;
            if (catalogo is not null)
            {
                modelo.Productos = catalogo.Items; modelo.Pagina = catalogo.Pagina;
                modelo.TamanoPagina = catalogo.TamanoPagina; modelo.TotalItems = catalogo.TotalItems;
                modelo.TotalPaginas = catalogo.TotalPaginas;
            }
        }
        catch (HttpRequestException) { modelo.Error = "El catálogo no está disponible en este momento. Podés reintentar."; }
        catch (TaskCanceledException) { modelo.Error = "La consulta tardó demasiado. Podés reintentar."; }
        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        try
        {
            var producto = await _productosApiService.ObtenerPorIdAsync(id);

            if (producto is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("NoEncontrado");
            }

            var categorias = await _categoriasApiService.ObtenerTodosAsync();
            return View(new ProductoDetalleViewModel { IdProducto = producto.IdProducto, Nombre = producto.Nombre,
                Descripcion = producto.Descripcion, Precio = producto.Precio, Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl,
                CategoriaNombre = categorias.FirstOrDefault(c => c.IdCategoria == producto.IdCategoria)?.Nombre ?? "Sin categoría" });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["Error"] = "No pudimos cargar el producto. Intentá nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear()
    {
        await CargarOpcionesAsync();
        return View(new ProductoRequest());
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoRequest request)
    {
        if (!ModelState.IsValid)
        {
            await CargarOpcionesAsync(request.IdProveedor);
            return View(request);
        }

        var response = await _productosApiService.CrearAsync(request.Nombre, request.Descripcion, request.Precio, request.Stock, request.IdCategoria, request.IdProveedor);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            await CargarOpcionesAsync(request.IdProveedor);
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
        await CargarOpcionesAsync(producto.IdProveedor);
        return View(new ProductoRequest { Nombre = producto.Nombre, Descripcion = producto.Descripcion, Precio = producto.Precio, Stock = producto.Stock, IdCategoria = producto.IdCategoria, IdProveedor = producto.IdProveedor });
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProductoRequest request)
    {
        if (!ModelState.IsValid)
        {
            await CargarOpcionesAsync(request.IdProveedor);
            return View(request);
        }

        var response = await _productosApiService.ActualizarAsync(id, request.Nombre, request.Descripcion, request.Precio, request.Stock, request.IdCategoria, request.IdProveedor);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            await CargarOpcionesAsync(request.IdProveedor);
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
        return RedirectToAction(nameof(Index), new { texto });
    }

    [HttpGet]
    public async Task<IActionResult> Categoria(int id)
    {
        return RedirectToAction(nameof(Index), new { idCategoria = id });
    }

    [HttpGet]
    public async Task<IActionResult> Disponibles()
    {
        return RedirectToAction(nameof(Index), new { soloDisponibles = true });
    }

    private async Task CargarOpcionesAsync(int? idProveedorActual = null)
    {
        var categoriasTask = _categoriasApiService.ObtenerTodosAsync();
        var proveedoresTask = _proveedoresApiService.ObtenerTodosAsync();
        await Task.WhenAll(categoriasTask, proveedoresTask);

        var proveedores = (await proveedoresTask)
            .Where(proveedor => proveedor.Activo || proveedor.IdProveedor == idProveedorActual)
            .OrderBy(proveedor => proveedor.RazonSocial)
            .ToList();

        ViewBag.Categorias = await categoriasTask;
        ViewBag.Proveedores = proveedores;
        ViewBag.HayProveedoresActivos = proveedores.Any(proveedor => proveedor.Activo);
    }
}
