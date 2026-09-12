// ============================================================================
// MÓDULO: SOLICITUDES DE CATEGORÍAS
// RESPONSABILIDAD: Representar los datos enviados a la API al crear o editar.
// LÍMITE: Es un contrato de entrada del frontend y no una entidad de persistencia.
// ============================================================================
namespace Frontend.Models.Api.Requests;

public class CategoriaRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}
