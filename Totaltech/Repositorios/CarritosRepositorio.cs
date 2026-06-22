using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface ICarritosRepositorio
    {
        Task<List<Carrito>> ObtenerTodosAsync();
        Task<Carrito?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Carrito carrito);
        Task ActualizarAsync(Carrito carrito);
        Task EliminarAsync(Carrito carrito);
        Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario);
    }

    public class CarritosRepositorio : ICarritosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public CarritosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Carrito>> ObtenerTodosAsync()
        {
            return await _context.Carritos.ToListAsync();
        }

        public async Task<Carrito?> ObtenerPorIdAsync(int id)
        {
            return await _context.Carritos.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Carritos.AnyAsync(carrito => carrito.IdCarrito == id);
        }

        public async Task CrearAsync(Carrito carrito)
        {
            _context.Carritos.Add(carrito);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Carrito carrito)
        {
            _context.Carritos.Update(carrito);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Carrito carrito)
        {
            _context.Carritos.Remove(carrito);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return await _context.Carritos
                .Where(carrito => carrito.IdUsuario == idUsuario)
                .ToListAsync();
        }
    }
}
