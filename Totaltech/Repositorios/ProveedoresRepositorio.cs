using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IProveedoresRepositorio : IRepositorio<Proveedor>
    {
    }

    public class ProveedoresRepositorio : Repositorio<Proveedor>, IProveedoresRepositorio
    {
        public ProveedoresRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
