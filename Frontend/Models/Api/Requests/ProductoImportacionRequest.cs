namespace Frontend.Models.Api.Requests;

// Contrato UC-28 acordado; su productor Backend todavía está pendiente.
public sealed class ProductoImportacionFilaRequest
{
    public int NumeroFila { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal? Precio { get; set; }
    public int? Stock { get; set; }
    public string? CategoriaNombre { get; set; }
    public int? IdCategoria { get; set; }
}

public class ProductoImportacionPrevisualizarRequest
{
    public int IdProveedor { get; set; }
    public List<ProductoImportacionFilaRequest> Filas { get; set; } = [];
}

public sealed class ProductoImportacionConfirmarRequest : ProductoImportacionPrevisualizarRequest
{
    public Guid IdOperacion { get; set; }
}
