namespace Totaltech.Logica.DTOs;

public sealed class ActualizarCantidadCarritoDto
{
    public int Cantidad { get; set; }
}

public sealed class LineaCarritoResponse
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class CarritoResumenResponse
{
    public int? IdCarrito { get; set; }
    public List<LineaCarritoResponse> Items { get; set; } = [];
    public int CantidadTotal { get; set; }
    public decimal Total { get; set; }
}
