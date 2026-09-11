using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IProductosRepositorio
    {
        Task<List<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Producto producto);
        Task ActualizarAsync(Producto producto);
        Task EliminarAsync(Producto producto);
        Task<List<Producto>> BuscarAsync(string? texto);
        Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<List<Producto>> ObtenerDisponiblesAsync();
        Task<bool> DescontarStockAsync(int idProducto, int cantidad);
    }

    public class ProductosRepositorio : IProductosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public ProductosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos.ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Productos.AnyAsync(producto => producto.IdProducto == id);
        }

        public async Task CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Producto producto)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Producto>> BuscarAsync(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return await _context.Productos.ToListAsync();
            }

            return await _context.Productos
                .Where(producto => producto.Nombre.Contains(texto) || producto.Descripcion.Contains(texto))
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            return await _context.Productos
                .Where(producto => producto.IdCategoria == idCategoria)
                .ToListAsync();
        }

        public async Task<List<Producto>> ObtenerDisponiblesAsync()
        {
            return await _context.Productos
                .Where(producto => producto.Stock > 0)
                .ToListAsync();
        }

        public async Task<bool> DescontarStockAsync(int idProducto, int cantidad)
        {
            if (cantidad <= 0)
            {
                return false;
            }

            if (_context.Database.IsRelational())
            {
                var filasAfectadas = await _context.Productos
                    .Where(producto => producto.IdProducto == idProducto && producto.Stock >= cantidad)
                    .ExecuteUpdateAsync(actualizacion => actualizacion
                        .SetProperty(producto => producto.Stock, producto => producto.Stock - cantidad));

                return filasAfectadas == 1;
            }

            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto is null || producto.Stock < cantidad)
            {
                return false;
            }

            producto.Stock -= cantidad;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
