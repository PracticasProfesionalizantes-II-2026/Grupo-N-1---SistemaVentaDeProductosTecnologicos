using Frontend.Services;
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
}