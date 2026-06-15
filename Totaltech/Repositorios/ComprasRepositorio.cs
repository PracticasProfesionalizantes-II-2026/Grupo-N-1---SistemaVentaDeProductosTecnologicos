using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IComprasRepositorio : IRepositorio<Compra>
    {
    }

    public class ComprasRepositorio : Repositorio<Compra>, IComprasRepositorio
    {
        public ComprasRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
