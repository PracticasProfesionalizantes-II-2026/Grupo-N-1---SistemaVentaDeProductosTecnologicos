using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IProductosLogica : ILogica<Producto>
    {
        Task<List<Producto>> BuscarAsync(string? texto);
        Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<List<Producto>> ObtenerDisponiblesAsync();
        Task<bool> ActualizarStockAsync(int id, int stock);
    }

    public class ProductosLogica : Logica<Producto>, IProductosLogica
    {
        private readonly IProductosRepositorio _repositorio;

        public ProductosLogica(IProductosRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Producto>> BuscarAsync(string? texto)
        {
            return _repositorio.BuscarAsync(texto);
        }

        public Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            return _repositorio.ObtenerPorCategoriaAsync(idCategoria);
        }

        public Task<List<Producto>> ObtenerDisponiblesAsync()
        {
            return _repositorio.ObtenerDisponiblesAsync();
        }

        public async Task<bool> ActualizarStockAsync(int id, int stock)
        {
            var producto = await _repositorio.ObtenerPorIdAsync(id);

            if (producto is null)
            {
                return false;
            }

            producto.Stock = stock;
            await _repositorio.ActualizarAsync(producto);
            return true;
        }
    }
}
