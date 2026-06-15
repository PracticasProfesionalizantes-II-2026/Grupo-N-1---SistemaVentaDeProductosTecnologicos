using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDireccionesLogica : ILogica<Direccion>
    {
    }

    public class DireccionesLogica : Logica<Direccion>, IDireccionesLogica
    {
        public DireccionesLogica(IDireccionesRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
