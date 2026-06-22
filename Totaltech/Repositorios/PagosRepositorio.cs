using Totaltech.Datos;
using Totaltech.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Totaltech.Repositorios
{
    public interface IPagosRepositorio : IRepositorio<Pago>
    {
        Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido);
    }

    public class PagosRepositorio : Repositorio<Pago>, IPagosRepositorio
    {
        public PagosRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido)
        {
            // Permite ver todos los intentos o registros de pago asociados a un pedido.
            return await Context.Pagos
                .Where(pago => pago.IdPedido == idPedido)
                .ToListAsync();
        }
    }
}
