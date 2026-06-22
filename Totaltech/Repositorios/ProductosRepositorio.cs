using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IProductosRepositorio : IRepositorio<Producto>
    {
        Task<List<Producto>> BuscarAsync(string? texto);
        Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<List<Producto>> ObtenerDisponiblesAsync();
    }

    public class ProductosRepositorio : Repositorio<Producto>, IProductosRepositorio
    {
        public ProductosRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<List<Producto>> BuscarAsync(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return await Context.Productos.ToListAsync();
            }

            return await Context.Productos
                .Where(producto => producto.Nombre.Contains(texto) || producto.Descripcion.Contains(texto))
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            return await Context.Productos
                .Where(producto => producto.IdCategoria == idCategoria)
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerDisponiblesAsync()
        {
            // Un producto disponible es aquel que todavia tiene stock positivo.
            return await Context.Productos
                .Where(producto => producto.Stock > 0)
                .ToListAsync();
        }
    }
}
