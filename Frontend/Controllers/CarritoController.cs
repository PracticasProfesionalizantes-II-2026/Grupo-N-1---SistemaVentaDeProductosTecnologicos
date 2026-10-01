using System.Net;
using Frontend.Models.ViewModels.Carrito;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[Authorize]
public sealed class CarritoController : Controller
{
    private readonly ICarritosApiService _carritos;
    public CarritoController(ICarritosApiService carritos) => _carritos = carritos;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try { return View(CarritoViewModel.Desde(await _carritos.ObtenerActualAsync(cancellationToken))); }
        catch (HttpRequestException) { TempData["Error"] = "No pudimos cargar el carrito. Intentá nuevamente."; }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested) { TempData["Error"] = "El carrito tardó demasiado en responder. Intentá nuevamente."; }
        return View(new CarritoViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int idProducto, int cantidad = 1, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var resultado = await EjecutarAsync(() => _carritos.AgregarAsync(idProducto, cantidad, cancellationToken));
        if (!resultado.Exitoso) return Volver(returnUrl, idProducto, resultado.Error);
        TempData["Mensaje"] = "Producto agregado al carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarCantidad(int idProducto, int cantidad, CancellationToken cancellationToken)
    {
        var resultado = await EjecutarAsync(() => _carritos.ActualizarCantidadAsync(idProducto, cantidad, cancellationToken));
        TempData[resultado.Exitoso ? "Mensaje" : "Error"] = resultado.Exitoso ? "Cantidad actualizada." : resultado.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int idProducto, CancellationToken cancellationToken)
    {
        var resultado = await EjecutarAsync(() => _carritos.EliminarAsync(idProducto, cancellationToken));
        TempData[resultado.Exitoso ? "Mensaje" : "Error"] = resultado.Exitoso ? "Producto eliminado del carrito." : resultado.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task<CarritoApiResultado> EjecutarAsync(Func<Task<CarritoApiResultado>> accion)
    {
        try { return await accion(); }
        catch (TaskCanceledException) { return new(null, HttpStatusCode.RequestTimeout, "La operación tardó demasiado. Intentá nuevamente."); }
        catch (HttpRequestException) { return new(null, HttpStatusCode.ServiceUnavailable, "El carrito no está disponible. Intentá nuevamente."); }
    }

    private IActionResult Volver(string? returnUrl, int idProducto, string? error)
    {
        TempData["Error"] = error ?? "No pudimos agregar el producto al carrito.";
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Detalle", "Productos", new { id = idProducto });
    }
}
