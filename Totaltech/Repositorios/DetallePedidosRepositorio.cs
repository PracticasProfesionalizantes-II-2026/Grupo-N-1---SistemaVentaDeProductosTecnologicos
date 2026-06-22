using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IDetallePedidosRepositorio
    {
        Task<List<DetallePedido>> ObtenerTodosAsync();
        Task<DetallePedido?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(DetallePedido detalle);
        Task ActualizarAsync(DetallePedido detalle);
        Task EliminarAsync(DetallePedido detalle);
    }

    public class DetallePedidosRepositorio : IDetallePedidosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public DetallePedidosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<DetallePedido>> ObtenerTodosAsync()
        {
            return await _context.DetallePedidos.ToListAsync();
        }

        public async Task<DetallePedido?> ObtenerPorIdAsync(int id)
        {
            return await _context.DetallePedidos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.DetallePedidos.AnyAsync(detalle => detalle.IdDetallePedido == id);
        }

        public async Task CrearAsync(DetallePedido detalle)
        {
            _context.DetallePedidos.Add(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(DetallePedido detalle)
        {
            _context.DetallePedidos.Update(detalle);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(DetallePedido detalle)
        {
            _context.DetallePedidos.Remove(detalle);
            await _context.SaveChangesAsync();
        }
    }
}
