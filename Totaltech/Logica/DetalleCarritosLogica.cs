using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetalleCarritosLogica : ILogica<DetalleCarrito>
    {
        Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito);
    }

    public class DetalleCarritosLogica : Logica<DetalleCarrito>, IDetalleCarritosLogica
    {
        private readonly IDetalleCarritosRepositorio _repositorio;

        public DetalleCarritosLogica(IDetalleCarritosRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito)
        {
            return _repositorio.ObtenerPorCarritoAsync(idCarrito);
        }
    }
}
