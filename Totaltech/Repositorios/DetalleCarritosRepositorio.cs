using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IDetalleCarritosRepositorio : IRepositorio<DetalleCarrito>
    {
        Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito);
        Task<DetalleCarrito?> ObtenerPorCarritoYProductoAsync(int idCarrito, int idProducto);
        Task<bool> EliminarPorCarritoYProductoAsync(int idCarrito, int idProducto);
    }

    public class DetalleCarritosRepositorio : Repositorio<DetalleCarrito>, IDetalleCarritosRepositorio
    {
        public DetalleCarritosRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito)
        {
            return await Context.DetalleCarritos
                .Where(detalle => detalle.IdCarrito == idCarrito)
                .ToListAsync();
        }

        public async Task<DetalleCarrito?> ObtenerPorCarritoYProductoAsync(int idCarrito, int idProducto)
        {
            return await Context.DetalleCarritos
                .FirstOrDefaultAsync(detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == idProducto);
        }

        public async Task<bool> EliminarPorCarritoYProductoAsync(int idCarrito, int idProducto)
        {
            var detalles = await Context.DetalleCarritos
                .Where(detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == idProducto)
                .ToListAsync();

            if (detalles.Count == 0)
            {
                return false;
            }

            Context.DetalleCarritos.RemoveRange(detalles);
            await Context.SaveChangesAsync();
            return true;
        }
    }
}
