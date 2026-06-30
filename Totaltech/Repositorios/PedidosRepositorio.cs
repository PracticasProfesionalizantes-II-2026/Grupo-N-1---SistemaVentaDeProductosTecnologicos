using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IPedidosRepositorio
    {
        Task<List<Pedido>> ObtenerTodosAsync();
        Task<Pedido?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Pedido pedido);
        Task ActualizarAsync(Pedido pedido);
        Task EliminarAsync(Pedido pedido);
        Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado);
    }

    public class PedidosRepositorio : IPedidosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public PedidosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pedido>> ObtenerTodosAsync()
        {
            return await _context.Pedidos.ToListAsync();
        }

        public async Task<Pedido?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pedidos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Pedidos.AnyAsync(pedido => pedido.IdPedido == id);
        }

        public async Task CrearAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Pedido pedido)
        {
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return await _context.Pedidos
                .Where(pedido => pedido.IdUsuario == idUsuario)
                .ToListAsync();
        }

        public async Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado)
        {
            return await _context.Pedidos
                .Where(pedido => pedido.Estado == estado)
                .ToListAsync();
        }
    }
}
