using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IConsultasLogica
    {
        Task<List<Consulta>> ObtenerTodosAsync();
        Task<Consulta?> ObtenerPorIdAsync(int id);
        Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<string?> CrearAsync(Consulta consulta);
        Task<string?> ActualizarAsync(Consulta consulta);
        Task<bool> EliminarAsync(int id);
    }

    public class ConsultasLogica : IConsultasLogica
    {
        private readonly IConsultasRepositorio _repositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public ConsultasLogica(IConsultasRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
        }

        public Task<List<Consulta>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Consulta?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public async Task<string?> CrearAsync(Consulta consulta)
        {
            var error = await ValidarConsultaAsync(consulta);
            if (error is not null)
            {
                return error;
            }

            if (consulta.FechaConsulta == default)
            {
                consulta.FechaConsulta = DateTime.Now;
            }

            await _repositorio.CrearAsync(consulta);
            return null;
        }

        public async Task<string?> ActualizarAsync(Consulta consulta)
        {
            var error = await ValidarConsultaAsync(consulta);
            if (error is not null)
            {
                return error;
            }

            if (consulta.FechaConsulta == default)
            {
                consulta.FechaConsulta = DateTime.Now;
            }

            await _repositorio.ActualizarAsync(consulta);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var consulta = await _repositorio.ObtenerPorIdAsync(id);
            if (consulta is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(consulta);
            return true;
        }

        private async Task<string?> ValidarConsultaAsync(Consulta consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta.Email) || string.IsNullOrWhiteSpace(consulta.Mensaje))
            {
                return "El email y el mensaje son obligatorios.";
            }

            if (consulta.IdUsuario.HasValue && !await _usuariosRepositorio.ExisteAsync(consulta.IdUsuario.Value))
            {
                return "El usuario indicado no existe.";
            }

            return null;
        }
    }
}
