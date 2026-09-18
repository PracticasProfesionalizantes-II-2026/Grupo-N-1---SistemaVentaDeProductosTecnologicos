// ============================================================================
// MÓDULO: RESPUESTAS DE PROVEEDORES
// RESPONSABILIDAD: Deserializar la representación de proveedor recibida de la API.
// LÍMITE: Refleja el contrato HTTP consumido; no es una entidad rastreada por EF.
// ============================================================================
namespace Frontend.Models.Api.Responses;

public class ProveedorResponse
{
    public int IdProveedor { get; set; }

    public string RazonSocial { get; set; } = string.Empty;

    public string Cuit { get; set; } = string.Empty;

    public string EmailComercial { get; set; } = string.Empty;

    public string TelefonoComercial { get; set; } = string.Empty;

    public string CondicionIva { get; set; } = string.Empty;

    public int? IdDireccion { get; set; }

    public DireccionProveedorResponse? Direccion { get; set; }

    public int PlazoPagoDias { get; set; }

    public int TiempoEntregaDias { get; set; }

    public string MonedaPreferida { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public string NombreParaSeleccion => Activo
        ? RazonSocial
        : $"{RazonSocial} (inactivo)";
}

public class DireccionProveedorResponse
{
    public int IdDireccion { get; set; }
    public string Calle { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
}
