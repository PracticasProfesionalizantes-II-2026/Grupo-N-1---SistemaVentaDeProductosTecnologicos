// ============================================================================
// MÓDULO: PROVEEDORES
// RESPONSABILIDAD: Listar, consultar y gestionar proveedores desde MVC.
// ============================================================================
using Frontend.Models.Api.Requests;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class ProveedoresController : Controller
{
    private readonly ProveedoresApiService _proveedoresApiService;

    public ProveedoresController(ProveedoresApiService proveedoresApiService)
    {
        _proveedoresApiService = proveedoresApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var proveedores = await _proveedoresApiService.ObtenerTodosAsync();

        return View(proveedores);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var proveedor = await _proveedoresApiService.ObtenerPorIdAsync(id);

        if (proveedor is null)
        {
            return NotFound();
        }

        return View(proveedor);
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public IActionResult Crear() => View(new ProveedorRequest());

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProveedorRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var response = await _proveedoresApiService.CrearAsync(request);
        if (!response.IsSuccessStatusCode) { ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync()); return View(request); }
        TempData["Mensaje"] = "Proveedor creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id)
    {
        var proveedor = await _proveedoresApiService.ObtenerPorIdAsync(id);
        if (proveedor is null) return NotFound();
        return View(new ProveedorRequest { RazonSocial = proveedor.RazonSocial, Cuit = proveedor.Cuit, EmailComercial = proveedor.EmailComercial, TelefonoComercial = proveedor.TelefonoComercial, CondicionIva = proveedor.CondicionIva, IdDireccion = proveedor.IdDireccion, PlazoPagoDias = proveedor.PlazoPagoDias, TiempoEntregaDias = proveedor.TiempoEntregaDias, MonedaPreferida = proveedor.MonedaPreferida, Activo = proveedor.Activo });
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProveedorRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var response = await _proveedoresApiService.ActualizarAsync(id, request);
        if (!response.IsSuccessStatusCode) { ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync()); return View(request); }
        TempData["Mensaje"] = "Proveedor actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var response = await _proveedoresApiService.EliminarAsync(id);
        TempData[response.IsSuccessStatusCode ? "Mensaje" : "Error"] = response.IsSuccessStatusCode ? "Proveedor eliminado correctamente." : "No se pudo eliminar el proveedor.";
        return RedirectToAction(nameof(Index));
    }
}
