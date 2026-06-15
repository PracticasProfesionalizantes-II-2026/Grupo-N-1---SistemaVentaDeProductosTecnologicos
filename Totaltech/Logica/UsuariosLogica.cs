using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IUsuariosLogica : ILogica<Usuario>
    {
    }

    public class UsuariosLogica : Logica<Usuario>, IUsuariosLogica
    {
        public UsuariosLogica(IUsuariosRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
