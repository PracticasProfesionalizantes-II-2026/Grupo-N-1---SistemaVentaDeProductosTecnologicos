using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IDetalleCarritosRepositorio : IRepositorio<DetalleCarrito>
    {
        Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito);
        Task EliminarPorCarritoYProductoAsync(int idCarrito, int idProducto);
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

        public async Task EliminarPorCarritoYProductoAsync(int idCarrito, int idProducto)
        {
            var detalles = await Context.DetalleCarritos
                .Where(detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == idProducto)
                .ToListAsync();

            Context.DetalleCarritos.RemoveRange(detalles);
            await Context.SaveChangesAsync();
        }
    }
}
