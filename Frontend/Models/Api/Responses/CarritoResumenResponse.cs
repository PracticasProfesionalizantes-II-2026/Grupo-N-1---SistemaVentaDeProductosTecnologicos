namespace Frontend.Models.Api.Responses;

public sealed class CarritoLineaResponse
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public string? ImagenUrl { get; set; }
}

public sealed class CarritoResumenResponse
{
    public int? IdCarrito { get; set; }
    public List<CarritoLineaResponse> Items { get; set; } = [];
    public int CantidadTotal { get; set; }
    public decimal Total { get; set; }
}
