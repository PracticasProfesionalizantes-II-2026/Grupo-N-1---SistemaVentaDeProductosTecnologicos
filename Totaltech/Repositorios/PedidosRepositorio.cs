using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IPedidosRepositorio : IRepositorio<Pedido>
    {
        Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado);
    }

    public class PedidosRepositorio : Repositorio<Pedido>, IPedidosRepositorio
    {
        public PedidosRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return await Context.Pedidos
                .Where(pedido => pedido.IdUsuario == idUsuario)
                .ToListAsync();
        }

        public async Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado)
        {
            return await Context.Pedidos
                .Where(pedido => pedido.Estado == estado)
                .ToListAsync();
        }
    }
}
