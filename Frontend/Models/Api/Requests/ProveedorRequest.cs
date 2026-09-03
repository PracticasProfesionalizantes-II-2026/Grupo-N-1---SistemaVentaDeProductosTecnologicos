// ============================================================================
// MÓDULO: PROVEEDORES
// RESPONSABILIDAD: Representar el contrato real de alta y edición de proveedor.
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Api.Requests;

public class ProveedorRequest
{
    public string RazonSocial { get; set; } = string.Empty;
    [Required] public string Cuit { get; set; } = string.Empty;
    [Required, EmailAddress] public string EmailComercial { get; set; } = string.Empty;
    public string TelefonoComercial { get; set; } = string.Empty;
    [Required] public string CondicionIva { get; set; } = string.Empty;
    public int? IdDireccion { get; set; }
    [Range(0, int.MaxValue)] public int PlazoPagoDias { get; set; }
    [Range(0, int.MaxValue)] public int TiempoEntregaDias { get; set; }
    [Required] public string MonedaPreferida { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
