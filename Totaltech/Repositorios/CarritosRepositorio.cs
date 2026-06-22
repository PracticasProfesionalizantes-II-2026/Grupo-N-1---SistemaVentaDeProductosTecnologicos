using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface ICarritosRepositorio : IRepositorio<Carrito>
    {
        Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario);
    }

    public class CarritosRepositorio : Repositorio<Carrito>, ICarritosRepositorio
    {
        public CarritosRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            // Consulta util para ver los carritos que pertenecen a un cliente.
            return await Context.Carritos
                .Where(carrito => carrito.IdUsuario == idUsuario)
                .ToListAsync();
        }
    }
}
