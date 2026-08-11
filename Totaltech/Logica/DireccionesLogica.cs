using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDireccionesLogica
    {
        Task<List<Direccion>> ObtenerTodosAsync();
        Task<Direccion?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Direccion direccion);
        Task<string?> ActualizarAsync(Direccion direccion);
        Task<bool> EliminarAsync(int id);
    }

    public class DireccionesLogica : IDireccionesLogica
    {
        private readonly IDireccionesRepositorio _repositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public DireccionesLogica(IDireccionesRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
        }

        public Task<List<Direccion>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Direccion?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Direccion direccion)
        {
            var error = await ValidarDireccionAsync(direccion);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.CrearAsync(direccion);
            return null;
        }

        public async Task<string?> ActualizarAsync(Direccion direccion)
        {
            var error = await ValidarDireccionAsync(direccion);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.ActualizarAsync(direccion);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var direccion = await _repositorio.ObtenerPorIdAsync(id);
            if (direccion is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(direccion);
            return true;
        }

        private async Task<string?> ValidarDireccionAsync(Direccion direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion.Calle) || string.IsNullOrWhiteSpace(direccion.Numero))
            {
                return "La calle y el numero son obligatorios.";
            }

            if (direccion.IdUsuario.HasValue && !await _usuariosRepositorio.ExisteAsync(direccion.IdUsuario.Value))
            {
                return "El usuario indicado no existe.";
            }

            return null;
        }
    }
}
