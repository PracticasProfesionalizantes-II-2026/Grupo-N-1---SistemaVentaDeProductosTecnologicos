// ============================================================================
// MÓDULO: ADMINISTRACIÓN
// RESPONSABILIDAD: Mostrar el punto de entrada del panel administrativo.
// ACCESO: Restringido a usuarios autenticados con el rol Admin.
// ============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models.ViewModels.Administracion;
using Frontend.Services;

namespace Frontend.Controllers;

[Authorize(Roles = "Admin")]
public class AdministracionController : Controller
{
    private readonly AdministracionApiService _administracionApiService;

    public AdministracionController(AdministracionApiService administracionApiService)
    {
        _administracionApiService = administracionApiService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var metricas = await _administracionApiService.ObtenerMetricasAsync(cancellationToken);
        return View(new AdminDashboardViewModel
        {
            TotalProductos = metricas.TotalProductos,
            TotalUsuarios = metricas.TotalUsuarios,
            PedidosPendientes = metricas.PedidosPendientes
        });
    }
}
