using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IPagosRepositorio : IRepositorio<Pago>
    {
    }

    public class PagosRepositorio : Repositorio<Pago>, IPagosRepositorio
    {
        public PagosRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
