using Totaltech.Entidades;
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

        public ConsultasLogica(IConsultasRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public new async Task CrearAsync(Consulta consulta)
        {
            if (consulta.FechaConsulta == default)
            {
                consulta.FechaConsulta = DateTime.Now;
            }

            await _repositorio.CrearAsync(consulta);
        }
    }
}
