using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IComprasLogica : ILogica<Compra>
    {
    }

    public class ComprasLogica : Logica<Compra>, IComprasLogica
    {
        public ComprasLogica(IComprasRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
