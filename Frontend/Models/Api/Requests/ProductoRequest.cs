// ============================================================================
// MÓDULO: SOLICITUDES DE PRODUCTOS
// RESPONSABILIDAD: Representar y validar los datos de alta o edición para la API.
// LÍMITE: Es un contrato de entrada del frontend y no una entidad de persistencia.
// ============================================================================
using System.ComponentModel.DataAnnotations;
namespace Frontend.Models.Api.Requests;
public class ProductoRequest
{
    [Required] public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal Precio { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    [Range(1, int.MaxValue)] public int IdCategoria { get; set; }
    [Range(1, int.MaxValue)] public int IdProveedor { get; set; }
}
