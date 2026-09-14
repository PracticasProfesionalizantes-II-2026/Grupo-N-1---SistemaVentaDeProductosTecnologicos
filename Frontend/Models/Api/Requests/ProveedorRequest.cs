// ============================================================================
// MÓDULO: SOLICITUDES DE PROVEEDORES
// RESPONSABILIDAD: Representar y validar el alta o edición enviada a la API.
// LÍMITE: Es un contrato de entrada del frontend y no una entidad de persistencia.
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Api.Requests;

public class ProveedorRequest
{
    [Required] public string RazonSocial { get; set; } = string.Empty;
    [Required] public string Cuit { get; set; } = string.Empty;
    [Required, EmailAddress] public string EmailComercial { get; set; } = string.Empty;
    public string TelefonoComercial { get; set; } = string.Empty;
    [Required] public string CondicionIva { get; set; } = string.Empty;
    [Required] public DireccionProveedorRequest Direccion { get; set; } = new();
    [Range(0, int.MaxValue)] public int PlazoPagoDias { get; set; }
    [Range(0, int.MaxValue)] public int TiempoEntregaDias { get; set; }
    [Required] public string MonedaPreferida { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class DireccionProveedorRequest
{
    [Required(ErrorMessage = "La calle es obligatoria.")]
    public string Calle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número es obligatorio.")]
    public string Numero { get; set; } = string.Empty;

    public string? Ciudad { get; set; }
    public string? Provincia { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
}
