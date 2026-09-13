// ============================================================================
// MÓDULO: ADMINISTRACIÓN
// RESPONSABILIDAD: Mostrar el punto de entrada del panel administrativo.
// ACCESO: Restringido a usuarios autenticados con el rol Admin.
// ============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Frontend.Controllers;
[Authorize(Roles="Admin")]
public class AdministracionController : Controller
{
 public IActionResult Index()=>View();
}
