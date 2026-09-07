using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.UnitTests.Support;

namespace Totaltech.UnitTests.Logica;

public sealed class UsuariosLogicaTests
{
    [Fact]
    public async Task RegistrarAsync_FuerzaClienteYHasheaLaContrasena()
    {
        const string contrasena = "Cliente123456";
        var repositorio = new FakeUsuariosRepositorio();
        var logica = new UsuariosLogica(repositorio);
        var usuario = CrearUsuario("cliente@test.local", contrasena, RolUsuario.Administrador);

        var error = await logica.RegistrarAsync(usuario);
        var login = await logica.LoginAsync(new LoginDto
        {
            Email = usuario.Email,
            Contrasena = contrasena
        });

        Assert.Null(error);
        Assert.Equal(RolUsuario.Cliente, usuario.Rol);
        Assert.NotEqual(contrasena, usuario.Contrasena);
        Assert.NotNull(login);
        Assert.Equal(usuario.IdUsuario, login.IdUsuario);
    }

    [Fact]
    public async Task LoginAsync_RechazaUnaContrasenaIncorrecta()
    {
        var repositorio = new FakeUsuariosRepositorio();
        var logica = new UsuariosLogica(repositorio);
        var usuario = CrearUsuario("login@test.local", "Correcta123456", RolUsuario.Cliente);
        await logica.RegistrarAsync(usuario);

        var resultado = await logica.LoginAsync(new LoginDto
        {
            Email = usuario.Email,
            Contrasena = "Incorrecta123456"
        });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AsegurarAdministradorAsync_EsIdempotenteYAutenticable()
    {
        const string email = "Admin@admin.com";
        const string contrasena = "Admin123456789";
        var repositorio = new FakeUsuariosRepositorio();
        var logica = new UsuariosLogica(repositorio);

        await logica.AsegurarAdministradorAsync(email, contrasena);
        await logica.AsegurarAdministradorAsync(email, contrasena);
        var login = await logica.LoginAsync(new LoginDto
        {
            Email = email,
            Contrasena = contrasena
        });

        var administrador = Assert.Single(repositorio.Usuarios);
        Assert.Equal(RolUsuario.Administrador, administrador.Rol);
        Assert.NotEqual(contrasena, administrador.Contrasena);
        Assert.NotNull(login);
        Assert.Equal(RolUsuario.Administrador, login.Rol);
    }

    private static Usuario CrearUsuario(string email, string contrasena, RolUsuario rol)
    {
        return new Usuario
        {
            Nombre = "Usuario",
            Apellido = "Prueba",
            Email = email,
            Contrasena = contrasena,
            Telefono = "1111111111",
            Rol = rol
        };
    }
}
