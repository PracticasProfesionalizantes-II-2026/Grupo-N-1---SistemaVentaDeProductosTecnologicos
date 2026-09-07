using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.UnitTests.Support;

internal sealed class FakeUsuariosRepositorio : IUsuariosRepositorio
{
    private readonly List<Usuario> _usuarios = [];
    private int _proximoId = 1;

    public IReadOnlyList<Usuario> Usuarios => _usuarios;

    public Task<List<Usuario>> ObtenerTodosAsync()
    {
        return Task.FromResult(_usuarios.ToList());
    }

    public Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return Task.FromResult(_usuarios.SingleOrDefault(usuario => usuario.IdUsuario == id));
    }

    public Task<bool> ExisteAsync(int id)
    {
        return Task.FromResult(_usuarios.Any(usuario => usuario.IdUsuario == id));
    }

    public Task CrearAsync(Usuario usuario)
    {
        if (usuario.IdUsuario == 0)
        {
            usuario.IdUsuario = _proximoId++;
        }

        _usuarios.Add(usuario);
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Usuario usuario)
    {
        return Task.CompletedTask;
    }

    public Task EliminarAsync(Usuario usuario)
    {
        _usuarios.Remove(usuario);
        return Task.CompletedTask;
    }

    public Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        var usuario = _usuarios.SingleOrDefault(candidato =>
            string.Equals(candidato.Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(usuario);
    }
}
