using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class ConsultasController : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Contacto() => View();

    [HttpGet, AllowAnonymous]
    public IActionResult Garantia() => View();
}
