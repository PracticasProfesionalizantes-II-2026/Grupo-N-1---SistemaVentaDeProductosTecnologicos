namespace Frontend.Models.ViewModels.Productos;

public sealed class ProductoDetalleViewModel
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
}
