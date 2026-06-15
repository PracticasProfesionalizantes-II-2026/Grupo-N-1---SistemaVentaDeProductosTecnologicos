using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IUsuariosRepositorio : IRepositorio<Usuario>
    {
    }

    public class UsuariosRepositorio : Repositorio<Usuario>, IUsuariosRepositorio
    {
        public UsuariosRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
