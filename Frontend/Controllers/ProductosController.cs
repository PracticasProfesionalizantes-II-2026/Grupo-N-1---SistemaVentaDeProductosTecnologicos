// ============================================================================
// MÓDULO: PRODUCTOS
// RESPONSABILIDAD: Coordinar catálogo, detalle y mantenimiento de productos.
// INTEGRACIÓN: Delega productos y categorías en sus servicios de API respectivos.
// ACCESO: El catálogo es público; crear, editar y eliminar requieren rol Admin.
// ============================================================================
using System.Net;
using System.Text.Json;
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
    private readonly ProductoImagenStorage _imagenStorage;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(
        ProductosApiService productosApiService,
        CategoriasApiService categoriasApiService,
        ProveedoresApiService proveedoresApiService,
        ProductoImagenStorage imagenStorage,
        ILogger<ProductosController> logger)
    {
        _productosApiService = productosApiService;
        _categoriasApiService = categoriasApiService;
        _proveedoresApiService = proveedoresApiService;
        _imagenStorage = imagenStorage;
        _logger = logger;
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
                if (User.IsInRole("Admin") && pagina > Math.Max(1, catalogo.TotalPaginas))
                {
                    return RedirectToAction(nameof(Index), new
                    {
                        texto, idCategoria, precioMin, precioMax, soloDisponibles,
                        pagina = Math.Max(1, catalogo.TotalPaginas), tamanoPagina
                    });
                }
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
    public async Task<IActionResult> Editar(int id, string? returnUrl)
    {
        var regreso = RegresoAlCatalogo(returnUrl);
        try
        {
            var producto = await _productosApiService.ObtenerPorIdAsync(id);
            if (producto is null) return ProductoNoEncontrado();
            await CargarOpcionesAsync(producto.IdProveedor);
            return View(new ProductoEdicionViewModel
            {
                IdProducto = id, Nombre = producto.Nombre, Descripcion = producto.Descripcion,
                Precio = producto.Precio, Stock = producto.Stock, IdCategoria = producto.IdCategoria,
                IdProveedor = producto.IdProveedor, ImagenActualUrl = producto.ImagenUrl, ReturnUrl = regreso
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["Error"] = "No pudimos cargar el producto. Intentá nuevamente.";
            return LocalRedirect(regreso);
        }
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> Editar(int id, ProductoEdicionViewModel request)
    {
        request.IdProducto = id;
        request.ReturnUrl = RegresoAlCatalogo(request.ReturnUrl);
        ModelState.Remove(nameof(request.ReturnUrl));
        string? imagenNueva = null;
        try
        {
            // La imagen vigente siempre se obtiene del servidor, nunca de un campo oculto.
            var producto = await _productosApiService.ObtenerPorIdAsync(id);
            if (producto is null) return ProductoNoEncontrado();
            request.ImagenActualUrl = producto.ImagenUrl;

            if (ModelState.IsValid && request.ImagenArchivo is not null)
            {
                var resultado = await _imagenStorage.GuardarAsync(request.ImagenArchivo, HttpContext.RequestAborted);
                imagenNueva = resultado.ImagenUrl;
                if (resultado.Error is not null)
                    ModelState.AddModelError(nameof(request.ImagenArchivo), resultado.Error);
            }

            if (ModelState.IsValid)
            {
                using var response = await _productosApiService.ActualizarAsync(id, request.Nombre,
                    request.Descripcion, request.Precio, request.Stock, request.IdCategoria,
                    request.IdProveedor, imagenNueva ?? producto.ImagenUrl);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Producto actualizado correctamente.";
                    return LocalRedirect(request.ReturnUrl);
                }

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized
                    or HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.Conflict
                    or HttpStatusCode.UnprocessableEntity)
                    DescartarImagenRechazada(imagenNueva);

                ModelState.AddModelError(string.Empty, await ErrorAlActualizarAsync(response));
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Un fallo de transporte no demuestra que la API haya rechazado el guardado.
            ModelState.AddModelError(string.Empty,
                "No pudimos confirmar el guardado. Revisá el producto en el catálogo antes de reintentar.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "No se pudo guardar la imagen del producto {IdProducto}.", id);
            ModelState.AddModelError(nameof(request.ImagenArchivo), "No pudimos guardar la imagen. Intentá nuevamente.");
        }

        if (request.ImagenArchivo is not null)
            ModelState.AddModelError(nameof(request.ImagenArchivo), "Volvé a seleccionar el archivo antes de guardar.");

        try { await CargarOpcionesAsync(request.IdProveedor); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ViewBag.Categorias = new List<CategoriaResponse>();
            ViewBag.Proveedores = new List<ProveedorResponse>();
            ViewBag.HayProveedoresActivos = false;
            ModelState.AddModelError(string.Empty, "No pudimos cargar las categorías y proveedores. Intentá nuevamente.");
        }
        return View(request);
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id, string? returnUrl)
    {
        try
        {
            using var response = await _productosApiService.EliminarAsync(id);
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Producto eliminado correctamente.";
            else
                TempData["Error"] = response.StatusCode switch
                {
                    HttpStatusCode.Conflict => "No se puede eliminar el producto porque tiene pedidos, carritos u otros datos relacionados.",
                    HttpStatusCode.NotFound => "El producto ya no existe.",
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Tu sesión no permite eliminar productos. Volvé a iniciar sesión como administrador.",
                    _ => "No se pudo eliminar el producto. Intentá nuevamente."
                };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["Error"] = "No pudimos confirmar la eliminación. Revisá el catálogo antes de reintentar.";
        }
        return LocalRedirect(RegresoAlCatalogo(returnUrl));
    }

    private IActionResult ProductoNoEncontrado()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NoEncontrado");
    }

    private string RegresoAlCatalogo(string? returnUrl)
    {
        var catalogo = Url.Action(nameof(Index), "Productos")!;
        if (!Url.IsLocalUrl(returnUrl)) return catalogo;
        var ruta = returnUrl!.Split('?', 2)[0].TrimEnd('/');
        return ruta.Equals(catalogo.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
            || ruta.Equals($"{Request.PathBase}/Productos/Index", StringComparison.OrdinalIgnoreCase)
            ? returnUrl : catalogo;
    }

    private void DescartarImagenRechazada(string? imagenUrl)
    {
        try { _imagenStorage.Eliminar(imagenUrl); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "No se pudo retirar una imagen rechazada.");
        }
    }

    private static async Task<string> ErrorAlActualizarAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            try
            {
                var mensaje = await response.Content.ReadFromJsonAsync<string>();
                if (!string.IsNullOrWhiteSpace(mensaje)) return mensaje;
            }
            catch (JsonException) { }
        }
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => "El producto ya no existe. Volvé al catálogo.",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Tu sesión no permite editar productos. Volvé a iniciar sesión como administrador.",
            _ => "No pudimos confirmar el guardado. Revisá el producto en el catálogo antes de reintentar."
        };
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
