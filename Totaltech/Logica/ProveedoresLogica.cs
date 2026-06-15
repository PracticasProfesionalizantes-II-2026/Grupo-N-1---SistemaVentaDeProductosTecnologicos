using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IProveedoresLogica : ILogica<Proveedor>
    {
    }

    public class ProveedoresLogica : Logica<Proveedor>, IProveedoresLogica
    {
        public ProveedoresLogica(IProveedoresRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
