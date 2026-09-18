namespace Totaltech.Logica.DTOs;

public sealed class FiltroCatalogoProductos
{
    public string? Texto { get; set; }
    public int? IdCategoria { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public bool SoloDisponibles { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 12;
}

public sealed class ProductoCatalogoResponse
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int IdCategoria { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public int IdProveedor { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
}

public sealed class CatalogoProductosResponse
{
    public List<ProductoCatalogoResponse> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalItems { get; set; }
    public int TotalPaginas { get; set; }
}
