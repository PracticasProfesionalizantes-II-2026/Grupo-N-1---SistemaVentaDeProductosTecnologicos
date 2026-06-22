using Totaltech.Datos;
using Totaltech.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Totaltech.Repositorios
{
    public interface IComprasRepositorio
    {
        Task<List<Compra>> ObtenerTodosAsync();
        Task<Compra?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Compra compra);
        Task ActualizarAsync(Compra compra);
        Task EliminarAsync(Compra compra);
    }

    public class ComprasRepositorio : IComprasRepositorio
    {
        private readonly TotaltechDbContext _context;

        public ComprasRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Compra>> ObtenerTodosAsync()
        {
            return await _context.Compras.ToListAsync();
        }

        public async Task<Compra?> ObtenerPorIdAsync(int id)
        {
            return await _context.Compras.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Compras.AnyAsync(compra => compra.IdCompra == id);
        }

        public async Task CrearAsync(Compra compra)
        {
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Compra compra)
        {
            _context.Compras.Update(compra);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Compra compra)
        {
            _context.Compras.Remove(compra);
            await _context.SaveChangesAsync();
        }
    }
}
