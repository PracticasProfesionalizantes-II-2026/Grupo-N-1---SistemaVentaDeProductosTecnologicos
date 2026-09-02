namespace Frontend.Models.Api.Responses;

public class ProductoResponse
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public int IdCategoria { get; set; }

    public int IdProveedor { get; set; }
}