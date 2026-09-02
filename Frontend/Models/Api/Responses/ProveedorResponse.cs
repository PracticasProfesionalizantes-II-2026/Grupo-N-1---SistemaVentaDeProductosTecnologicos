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

    public int PlazoPagoDias { get; set; }

    public int TiempoEntregaDias { get; set; }

    public string MonedaPreferida { get; set; } = string.Empty;

    public bool Activo { get; set; }
}