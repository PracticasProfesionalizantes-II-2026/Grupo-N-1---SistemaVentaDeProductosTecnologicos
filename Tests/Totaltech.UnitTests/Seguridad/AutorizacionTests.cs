using System.Security.Claims;
using Totaltech.Entidades;
using Totaltech.Seguridad;

namespace Totaltech.UnitTests.Seguridad;

public sealed class AutorizacionTests
{
    [Fact]
    public void EsAdministrador_NoAutorizaPorEmail()
    {
        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim(ClaimTypes.Email, "Admin@admin.com"),
            new Claim(ClaimTypes.Role, nameof(RolUsuario.Cliente))
        ], "Test");

        var usuario = new ClaimsPrincipal(identidad);

        Assert.False(usuario.EsAdministrador());
    }

    [Fact]
    public void PuedeAcceder_AutorizaAlPropietarioYAlAdministrador()
    {
        var propietario = CrearPrincipal(10, RolUsuario.Cliente);
        var otroCliente = CrearPrincipal(11, RolUsuario.Cliente);
        var administrador = CrearPrincipal(12, RolUsuario.Administrador);

        Assert.True(propietario.PuedeAcceder(10));
        Assert.False(otroCliente.PuedeAcceder(10));
        Assert.True(administrador.PuedeAcceder(10));
    }

    private static ClaimsPrincipal CrearPrincipal(int idUsuario, RolUsuario rol)
    {
        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
            new Claim(ClaimTypes.Role, rol.ToString())
        ], "Test");

        return new ClaimsPrincipal(identidad);
    }
}
