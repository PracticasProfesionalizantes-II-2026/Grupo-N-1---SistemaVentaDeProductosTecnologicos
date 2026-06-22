using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IConsultasLogica : ILogica<Consulta>
    {
        Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario);
    }

    public class ConsultasLogica : Logica<Consulta>, IConsultasLogica
    {
        private readonly IConsultasRepositorio _repositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public ConsultasLogica(IConsultasRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
        }

        public Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public override async Task<ResultadoOperacion<Consulta>> CrearValidadoAsync(Consulta consulta)
        {
            var validacion = await ValidarConsultaAsync(consulta);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Consulta>.BadRequest(validacion.Error ?? "La consulta no es valida.");
            }

            if (consulta.FechaConsulta == default)
            {
                consulta.FechaConsulta = DateTime.Now;
            }

            await _repositorio.CrearAsync(consulta);
            return ResultadoOperacion<Consulta>.Ok(consulta);
        }

        public override async Task<ResultadoOperacion<Consulta>> ActualizarValidadoAsync(int id, Consulta consulta)
        {
            var validacion = await ValidarConsultaAsync(consulta);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Consulta>.BadRequest(validacion.Error ?? "La consulta no es valida.");
            }

            if (consulta.FechaConsulta == default)
            {
                consulta.FechaConsulta = DateTime.Now;
            }

            return await base.ActualizarValidadoAsync(id, consulta);
        }

        private async Task<ResultadoOperacion> ValidarConsultaAsync(Consulta consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta.Email) || string.IsNullOrWhiteSpace(consulta.Mensaje))
            {
                return ResultadoOperacion.BadRequest("El email y el mensaje son obligatorios.");
            }

            if (consulta.IdUsuario.HasValue && !await _usuariosRepositorio.ExisteAsync(consulta.IdUsuario.Value))
            {
                return ResultadoOperacion.BadRequest("El usuario indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
