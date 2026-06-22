using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IPagosRepositorio
    {
        Task<List<Pago>> ObtenerTodosAsync();
        Task<Pago?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Pago pago);
        Task ActualizarAsync(Pago pago);
        Task EliminarAsync(Pago pago);
        Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido);
    }

    public class PagosRepositorio : IPagosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public PagosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pago>> ObtenerTodosAsync()
        {
            return await _context.Pagos.ToListAsync();
        }

        public async Task<Pago?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pagos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Pagos.AnyAsync(pago => pago.IdPago == id);
        }

        public async Task CrearAsync(Pago pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Pago pago)
        {
            _context.Pagos.Update(pago);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Pago pago)
        {
            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido)
        {
            return await _context.Pagos
                .Where(pago => pago.IdPedido == idPedido)
                .ToListAsync();
        }
    }
}
