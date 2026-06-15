using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public class Logica<TEntidad> : ILogica<TEntidad> where TEntidad : class
    {
        protected readonly IRepositorio<TEntidad> Repositorio;

        public Logica(IRepositorio<TEntidad> repositorio)
        {
            Repositorio = repositorio;
        }

        public Task<List<TEntidad>> ObtenerTodosAsync()
        {
            return Repositorio.ObtenerTodosAsync();
        }

        public Task<TEntidad?> ObtenerPorIdAsync(int id)
        {
            return Repositorio.ObtenerPorIdAsync(id);
        }

        public Task CrearAsync(TEntidad entidad)
        {
            return Repositorio.CrearAsync(entidad);
        }

        public Task ActualizarAsync(TEntidad entidad)
        {
            return Repositorio.ActualizarAsync(entidad);
        }

        public Task EliminarAsync(TEntidad entidad)
        {
            return Repositorio.EliminarAsync(entidad);
        }
    }
}
