using Frontend.Models.Api.Requests;

namespace Frontend.Models.Api.Responses;

public sealed class ProductoImportacionErrorResponse
{
    public int? NumeroFila { get; set; }
    public string? Campo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class ProductoImportacionPrevisualizacionResponse
{
    public List<ProductoImportacionFilaRequest> Filas { get; set; } = [];
    public List<ProductoImportacionErrorResponse> Errores { get; set; } = [];
    public bool PuedeConfirmar { get; set; }
}

public sealed class ProductoImportacionConfirmacionResponse
{
    public Guid IdOperacion { get; set; }
    public int Importados { get; set; }
}
