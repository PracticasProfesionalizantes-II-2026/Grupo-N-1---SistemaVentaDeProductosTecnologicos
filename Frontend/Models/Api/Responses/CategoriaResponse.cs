// ============================================================================
// MÓDULO: RESPUESTAS DE CATEGORÍAS
// RESPONSABILIDAD: Deserializar la representación de categoría recibida de la API.
// LÍMITE: Refleja el contrato HTTP consumido; no es una entidad rastreada por EF.
// ============================================================================
namespace Frontend.Models.Api.Responses;

public class CategoriaResponse
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}
