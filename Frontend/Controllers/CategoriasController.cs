// Estructura reservada. Implementación pendiente.
using Frontend.Models.Api.Requests;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class CategoriasController : Controller
{
    private readonly CategoriasApiService _categoriasApiService;

    public CategoriasController(CategoriasApiService categoriasApiService)
    {
        _categoriasApiService = categoriasApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categorias = await _categoriasApiService.ObtenerTodosAsync();

        return View(categorias);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var categoria = await _categoriasApiService.ObtenerPorIdAsync(id);

        if (categoria is null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CategoriaRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CategoriaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var resultado = await _categoriasApiService.CrearAsync(request);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Error ?? "No se pudo crear la categoría.");

            return View(request);
        }

        TempData["Mensaje"] = "Categoría creada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _categoriasApiService.ObtenerPorIdAsync(id);

        if (categoria is null)
        {
            return NotFound();
        }

        var request = new CategoriaRequest
        {
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion
        };

        ViewBag.IdCategoria = id;

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CategoriaRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.IdCategoria = id;
            return View(request);
        }

        var resultado = await _categoriasApiService.ActualizarAsync(id, request);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Error ?? "No se pudo actualizar la categoría.");

            ViewBag.IdCategoria = id;

            return View(request);
        }

        TempData["Mensaje"] = "Categoría actualizada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _categoriasApiService.EliminarAsync(id);

        if (!resultado.Exitoso)
        {
            TempData["Error"] =
                resultado.Error ?? "No se pudo eliminar la categoría.";
        }
        else
        {
            TempData["Mensaje"] = "Categoría eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}