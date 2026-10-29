using System.ComponentModel.DataAnnotations;

namespace Totaltech.Entidades;

public sealed class AuditoriaUsuario
{
    [Key]
    public int IdAuditoriaUsuario { get; set; }
    public int IdActor { get; set; }
    public int IdUsuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
    public string CamposModificados { get; set; } = string.Empty;
    public RolUsuario? RolAnterior { get; set; }
    public RolUsuario? RolNuevo { get; set; }
    public bool? ActivoAnterior { get; set; }
    public bool? ActivoNuevo { get; set; }
}
