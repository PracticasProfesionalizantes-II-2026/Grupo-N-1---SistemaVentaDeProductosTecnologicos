using Totaltech.Logica.DTOs;

namespace Totaltech.Logica
{
    public interface ILogica<TEntidad> where TEntidad : class
    {
        Task<List<TEntidad>> ObtenerTodosAsync();
        Task<TEntidad?> ObtenerPorIdAsync(int id);
        Task CrearAsync(TEntidad entidad);
        Task ActualizarAsync(TEntidad entidad);
        Task EliminarAsync(TEntidad entidad);
        Task<ResultadoOperacion<TEntidad>> CrearValidadoAsync(TEntidad entidad);
        Task<ResultadoOperacion<TEntidad>> ActualizarValidadoAsync(int id, TEntidad entidad);
        Task<ResultadoOperacion> EliminarPorIdAsync(int id);
    }
}
