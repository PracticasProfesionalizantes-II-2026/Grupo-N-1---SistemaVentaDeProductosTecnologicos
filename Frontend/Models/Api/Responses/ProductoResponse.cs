// ============================================================================
// MÓDULO: RESPUESTAS DE PRODUCTOS
// RESPONSABILIDAD: Deserializar los datos de producto recibidos desde la API.
// LÍMITE: Refleja el contrato HTTP consumido; no es una entidad rastreada por EF.
// ============================================================================
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

    public string? ImagenUrl { get; set; }
}
