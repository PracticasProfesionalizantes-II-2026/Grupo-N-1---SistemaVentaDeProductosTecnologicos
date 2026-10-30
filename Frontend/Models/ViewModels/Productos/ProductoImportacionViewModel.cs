using Frontend.Models.Api.Responses;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.ViewModels.Productos;

public sealed class ProductoImportacionViewModel
{
    public IFormFile? Archivo { get; set; }
    public int? IdProveedor { get; set; }
    public Guid IdOperacion { get; set; }
    public List<ProductoImportacionFilaViewModel> Filas { get; set; } = [];
    [BindNever] public List<CategoriaResponse> Categorias { get; set; } = [];
    [BindNever] public List<ProveedorResponse> Proveedores { get; set; } = [];
    [BindNever] public List<string> Advertencias { get; set; } = [];
    [BindNever] public bool PuedeConfirmar { get; set; }
}

public sealed class ProductoImportacionFilaViewModel
{
    [DisplayFormat(ConvertEmptyStringToNull = false)] public string Nombre { get; set; } = string.Empty;
    [DisplayFormat(ConvertEmptyStringToNull = false)] public string Descripcion { get; set; } = string.Empty;
    // Texto editable: no perder el valor original cuando un número no se puede interpretar.
    [DisplayFormat(ConvertEmptyStringToNull = false)] public string Precio { get; set; } = string.Empty;
    [DisplayFormat(ConvertEmptyStringToNull = false)] public string Stock { get; set; } = string.Empty;
    [DisplayFormat(ConvertEmptyStringToNull = false)] public string CategoriaNombre { get; set; } = string.Empty;
    public int? IdCategoria { get; set; }
    [DisplayFormat(ConvertEmptyStringToNull = false)] public string Origen { get; set; } = string.Empty;
}
