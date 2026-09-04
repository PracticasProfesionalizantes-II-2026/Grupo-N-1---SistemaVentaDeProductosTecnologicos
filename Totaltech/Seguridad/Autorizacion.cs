using System.Security.Claims;
using Totaltech.Entidades;

namespace Totaltech.Seguridad;

public static class Autorizacion
{
    public const string PoliticaAdministrador = "Administrador";

    public static int? ObtenerIdUsuario(this ClaimsPrincipal usuario)
    {
        return int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;
    }

    public static bool EsAdministrador(this ClaimsPrincipal usuario)
    {
        return usuario.IsInRole(nameof(RolUsuario.Administrador));
    }

    public static bool PuedeAcceder(this ClaimsPrincipal usuario, int? idPropietario)
    {
        return usuario.EsAdministrador() ||
               idPropietario.HasValue && usuario.ObtenerIdUsuario() == idPropietario.Value;
    }
}
