using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface ICategoriasLogica
    {
        Task<List<Categoria>> ObtenerTodosAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Categoria categoria);
        Task<string?> ActualizarAsync(Categoria categoria);
        Task<bool> EliminarAsync(int id);
    }

    public class CategoriasLogica : ICategoriasLogica
    {
        private readonly ICategoriasRepositorio _repositorio;

        public CategoriasLogica(ICategoriasRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Categoria>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Categoria categoria)
        {
            var error = ValidarCategoria(categoria);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.CrearAsync(categoria);
            return null;
        }

        public async Task<string?> ActualizarAsync(Categoria categoria)
        {
            var error = ValidarCategoria(categoria);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.ActualizarAsync(categoria);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var categoria = await _repositorio.ObtenerPorIdAsync(id);
            if (categoria is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(categoria);
            return true;
        }

        private static string? ValidarCategoria(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return "El nombre de la categoria es obligatorio.";
            }

            return null;
        }
    }
}
