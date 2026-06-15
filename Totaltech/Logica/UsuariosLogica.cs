using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IUsuariosLogica : ILogica<Usuario>
    {
        Task<Usuario?> LoginAsync(LoginDto dto);
        Task<Usuario?> RegistrarAsync(Usuario usuario);
        Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto);
    }

    public class UsuariosLogica : Logica<Usuario>, IUsuariosLogica
    {
        private readonly IUsuariosRepositorio _repositorio;

        public UsuariosLogica(IUsuariosRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<Usuario?> LoginAsync(LoginDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(dto.Email);

            if (usuario is null || usuario.Contrasena != dto.Contrasena)
            {
                return null;
            }

            return usuario;
        }

        public async Task<Usuario?> RegistrarAsync(Usuario usuario)
        {
            var existente = await _repositorio.ObtenerPorEmailAsync(usuario.Email);

            if (existente is not null)
            {
                return null;
            }

            if (usuario.FechaRegistro == default)
            {
                usuario.FechaRegistro = DateTime.Now;
            }

            await _repositorio.CrearAsync(usuario);
            return usuario;
        }

        public async Task<bool> RecuperarContrasenaAsync(RecuperarContrasenaDto dto)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(dto.Email);
            return usuario is not null;
        }
    }
}
