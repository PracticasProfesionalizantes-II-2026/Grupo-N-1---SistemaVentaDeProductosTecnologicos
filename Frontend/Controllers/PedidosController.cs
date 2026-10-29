using System.Security.Claims;
using Frontend.Filters;
using Frontend.Models.ViewModels.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[Authorize]
public sealed class PedidosController(IConfiguration configuration) : Controller
{
    [HttpGet, CompraDemoHabilitada]
    public IActionResult Index()
    {
        if (!configuration.GetValue<bool>("CompraDemo:Enabled")) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Challenge();
        return View(new PedidoListadoViewModel(idUsuario));
    }

    [HttpGet, CompraDemoHabilitada]
    public IActionResult Detalle(string? id)
    {
        if (!configuration.GetValue<bool>("CompraDemo:Enabled")) return NotFound();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario)) return Challenge();
        return View(new PedidoDetalleViewModel(idUsuario, id ?? string.Empty));
    }
}
