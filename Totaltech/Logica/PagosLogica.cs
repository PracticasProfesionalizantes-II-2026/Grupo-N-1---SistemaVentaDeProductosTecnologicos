using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPagosLogica : ILogica<Pago>
    {
        Task<bool> ActualizarEstadoAsync(int id, EstadoPago estado);
    }

    public class PagosLogica : Logica<Pago>, IPagosLogica
    {
        private readonly IPagosRepositorio _repositorio;

        public PagosLogica(IPagosRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<bool> ActualizarEstadoAsync(int id, EstadoPago estado)
        {
            var pago = await _repositorio.ObtenerPorIdAsync(id);

            if (pago is null)
            {
                return false;
            }

            pago.Estado = estado;
            await _repositorio.ActualizarAsync(pago);
            return true;
        }
    }
}
