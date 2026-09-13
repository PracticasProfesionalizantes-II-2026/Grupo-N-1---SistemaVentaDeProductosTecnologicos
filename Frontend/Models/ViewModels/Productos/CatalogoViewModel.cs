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
}
