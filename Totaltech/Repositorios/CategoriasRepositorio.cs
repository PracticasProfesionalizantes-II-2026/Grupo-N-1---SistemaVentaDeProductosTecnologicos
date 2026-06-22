using Totaltech.Datos;
using Totaltech.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Totaltech.Repositorios
{
    public interface ICategoriasRepositorio
    {
        Task<List<Categoria>> ObtenerTodosAsync();
        Task<Categoria?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Categoria categoria);
        Task ActualizarAsync(Categoria categoria);
        Task EliminarAsync(Categoria categoria);
    }

    public class CategoriasRepositorio : ICategoriasRepositorio
    {
        private readonly TotaltechDbContext _context;

        public CategoriasRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ObtenerTodosAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Categorias.AnyAsync(categoria => categoria.IdCategoria == id);
        }

        public async Task CrearAsync(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}
