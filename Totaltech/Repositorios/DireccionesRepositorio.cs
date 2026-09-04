using Totaltech.Datos;
using Totaltech.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Totaltech.Repositorios
{
    public interface IDireccionesRepositorio
    {
        Task<List<Direccion>> ObtenerTodosAsync();
        Task<Direccion?> ObtenerPorIdAsync(int id);
        Task<List<Direccion>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Direccion direccion);
        Task ActualizarAsync(Direccion direccion);
        Task EliminarAsync(Direccion direccion);
    }

    public class DireccionesRepositorio : IDireccionesRepositorio
    {
        private readonly TotaltechDbContext _context;

        public DireccionesRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Direccion>> ObtenerTodosAsync()
        {
            return await _context.Direcciones.ToListAsync();
        }

        public async Task<Direccion?> ObtenerPorIdAsync(int id)
        {
            return await _context.Direcciones.FindAsync(id);
        }

        public Task<List<Direccion>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _context.Direcciones
                .Where(direccion => direccion.IdUsuario == idUsuario)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Direcciones.AnyAsync(direccion => direccion.IdDireccion == id);
        }

        public async Task CrearAsync(Direccion direccion)
        {
            _context.Direcciones.Add(direccion);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Direccion direccion)
        {
            _context.Direcciones.Update(direccion);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Direccion direccion)
        {
            _context.Direcciones.Remove(direccion);
            await _context.SaveChangesAsync();
        }
    }
}
