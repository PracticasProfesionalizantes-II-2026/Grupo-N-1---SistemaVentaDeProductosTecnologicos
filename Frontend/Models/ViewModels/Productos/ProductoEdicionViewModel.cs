using System.ComponentModel.DataAnnotations;
using Frontend.Models.Api.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Frontend.Models.ViewModels.Productos;

public sealed class ProductoEdicionViewModel : ProductoRequest
{
    [BindNever] public int IdProducto { get; set; }
    [BindNever] public string? ImagenActualUrl { get; set; }

    [Display(Name = "Imagen de referencia")]
    public IFormFile? ImagenArchivo { get; set; }

    public string? ReturnUrl { get; set; }
}
