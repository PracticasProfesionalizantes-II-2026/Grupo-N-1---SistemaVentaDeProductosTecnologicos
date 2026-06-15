using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface ICategoriasRepositorio : IRepositorio<Categoria>
    {
    }

    public class CategoriasRepositorio : Repositorio<Categoria>, ICategoriasRepositorio
    {
        public CategoriasRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
