namespace Totaltech.Logica
{
    public interface ILogica<TEntidad> where TEntidad : class
    {
        Task<List<TEntidad>> ObtenerTodosAsync();
        Task<TEntidad?> ObtenerPorIdAsync(int id);
        Task CrearAsync(TEntidad entidad);
        Task ActualizarAsync(TEntidad entidad);
        Task EliminarAsync(TEntidad entidad);
    }
}
