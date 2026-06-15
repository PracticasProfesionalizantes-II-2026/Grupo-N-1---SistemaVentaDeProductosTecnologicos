using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface ICategoriasLogica : ILogica<Categoria>
    {
    }

    public class CategoriasLogica : Logica<Categoria>, ICategoriasLogica
    {
        public CategoriasLogica(ICategoriasRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
