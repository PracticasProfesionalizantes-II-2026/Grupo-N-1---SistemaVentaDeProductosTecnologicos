namespace Frontend.Models.Api.Responses;

public sealed class CatalogoProductosResponse
{
    public List<ProductoResponse> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalItems { get; set; }
    public int TotalPaginas { get; set; }
}

public sealed class ErrorCatalogoResponse
{
    public string Codigo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
