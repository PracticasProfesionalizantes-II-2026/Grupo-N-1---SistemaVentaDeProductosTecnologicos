using System.Security.Claims;
using Frontend.Filters;
using Frontend.Models.ViewModels.Checkout;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[Authorize]
public sealed class CheckoutController(ICarritosApiService carritos, IConfiguration configuration) : Controller
{
    [HttpGet, CompraDemoHabilitada]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("CompraDemo:Enabled")) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Challenge();
        var modelo = new CheckoutViewModel { IdUsuario = idUsuario };
        try
        {
            // Sólo consultar: confirmar el carrito real también descontaría stock.
            modelo.Carrito = await carritos.ObtenerActualAsync(cancellationToken);
            if (modelo.Carrito is null) modelo.Error = "No pudimos cargar el carrito. Volvé a intentarlo.";
        }
        catch (HttpRequestException)
        {
            modelo.Error = "No pudimos cargar el carrito. Volvé a intentarlo.";
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            modelo.Error = "El carrito tardó demasiado en responder. Volvé a intentarlo.";
        }
        return View(modelo);
    }

    [HttpGet, CompraDemoHabilitada]
    public IActionResult Confirmacion(string? id)
    {
        if (!configuration.GetValue<bool>("CompraDemo:Enabled")) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Challenge();
        return View(new ConfirmacionCompraViewModel(idUsuario, id ?? string.Empty));
    }
}
