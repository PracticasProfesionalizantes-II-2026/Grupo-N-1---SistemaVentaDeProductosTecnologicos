using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Frontend.Controllers;
[Authorize(Roles="Admin")]
public class AdministracionController : Controller
{
 public IActionResult Index()=>View();
}
