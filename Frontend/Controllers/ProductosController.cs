using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class ProductosController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
