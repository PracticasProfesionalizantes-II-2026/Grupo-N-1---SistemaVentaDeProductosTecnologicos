using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IDetalleCarritosRepositorio
    {
        Task<List<DetalleCarrito>> ObtenerTodosAsync();
        Task<DetalleCarrito?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(DetalleCarrito detalle);
        Task ActualizarAsync(DetalleCarrito detalle);
        Task EliminarAsync(DetalleCarrito detalle);
        Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito);
        Task<DetalleCarrito?> ObtenerPorCarritoYProductoAsync(int idCarrito, int idProducto);
        Task<bool> EliminarPorCarritoYProductoAsync(int idCarrito, int idProducto);
    }

    public class DetalleCarritosRepositorio : IDetalleCarritosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public DetalleCarritosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<DetalleCarrito>> ObtenerTodosAsync()
        {
            return await _context.DetalleCarritos.ToListAsync();
        }

        public async Task<DetalleCarrito?> ObtenerPorIdAsync(int id)
        {
            return await _context.DetalleCarritos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.DetalleCarritos.AnyAsync(detalle => detalle.IdDetalleCarrito == id);
        }

        public async Task CrearAsync(DetalleCarrito detalle)
        {
            _context.DetalleCarritos.Add(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(DetalleCarrito detalle)
        {
            _context.DetalleCarritos.Update(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(DetalleCarrito detalle)
        {
            _context.DetalleCarritos.Remove(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito)
        {
            return await _context.DetalleCarritos
                .Where(detalle => detalle.IdCarrito == idCarrito)
                .ToListAsync();
        }

        public async Task<DetalleCarrito?> ObtenerPorCarritoYProductoAsync(int idCarrito, int idProducto)
        {
            return await _context.DetalleCarritos
                .FirstOrDefaultAsync(detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == idProducto);
        }

        public async Task<bool> EliminarPorCarritoYProductoAsync(int idCarrito, int idProducto)
        {
            var detalles = await _context.DetalleCarritos
                .Where(detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == idProducto)
                .ToListAsync();

            if (detalles.Count == 0)
            {
                return false;
            }

            _context.DetalleCarritos.RemoveRange(detalles);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
