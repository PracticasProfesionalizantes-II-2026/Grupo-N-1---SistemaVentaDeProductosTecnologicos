// ============================================================================
// MÓDULO: CATÁLOGO DE PRODUCTOS
// RESPONSABILIDAD: Componer productos, categorías y filtro activo para la vista.
// LÍMITE: Sólo prepara presentación; la consulta de datos corresponde al servicio.
// ============================================================================
using Frontend.Models.Api.Responses;

namespace Frontend.Models.ViewModels.Productos;

public class CatalogoViewModel
{
    public List<ProductoResponse> Productos { get; set; } = [];

    public List<CategoriaResponse> Categorias { get; set; } = [];

    public int? CategoriaSeleccionadaId { get; set; }
    public string? Texto { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public bool SoloDisponibles { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPaginas { get; set; }
    public string? Error { get; set; }
}
