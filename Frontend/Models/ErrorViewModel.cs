// ============================================================================
// MÓDULO: ERRORES
// RESPONSABILIDAD: Proporcionar a la vista el identificador de una solicitud fallida.
// PRIVACIDAD: Sólo expone el identificador cuando existe; no contiene excepciones.
// ============================================================================
namespace Frontend.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
