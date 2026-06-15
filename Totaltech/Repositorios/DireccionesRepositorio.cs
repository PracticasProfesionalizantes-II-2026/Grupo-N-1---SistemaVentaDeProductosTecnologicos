using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IDireccionesRepositorio : IRepositorio<Direccion>
    {
    }

    public class DireccionesRepositorio : Repositorio<Direccion>, IDireccionesRepositorio
    {
        public DireccionesRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
