using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using Frontend.Models.Api.Requests;
namespace Frontend.Controllers;
[Authorize(Roles="Admin")]
public class AdministracionController(CategoriasApiService c, ProductosApiService p) : Controller
{
 public IActionResult Index()=>View();
 public async Task<IActionResult> Categorias()=>View(await c.ObtenerTodosAsync());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Categoria(string nombre,string descripcion){await c.CrearAsync(new CategoriaRequest { Nombre=nombre, Descripcion=descripcion });return RedirectToAction(nameof(Categorias));}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> EliminarCategoria(int id){TempData["Mensaje"]=(await c.EliminarAsync(id)).Estado==System.Net.HttpStatusCode.Conflict?"No se puede eliminar: tiene productos asignados.":"Categoría eliminada.";return RedirectToAction(nameof(Categorias));}
 public async Task<IActionResult> Productos()=>View(await p.ObtenerTodosAsync());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Producto(string nombre,string descripcion,decimal precio,int stock,int idCategoria,int idProveedor){await p.CrearAsync(nombre,descripcion,precio,stock,idCategoria,idProveedor);return RedirectToAction(nameof(Productos));}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> EliminarProducto(int id){await p.EliminarAsync(id);return RedirectToAction(nameof(Productos));}
}
