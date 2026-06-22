using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IProveedoresRepositorio
    {
        Task<List<Proveedor>> ObtenerTodosAsync();
        Task<Proveedor?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Proveedor proveedor);
        Task ActualizarAsync(Proveedor proveedor);
        Task EliminarAsync(Proveedor proveedor);
    }

    public class ProveedoresRepositorio : IProveedoresRepositorio
    {
        private readonly TotaltechDbContext _context;

        public ProveedoresRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return await _context.Proveedores.ToListAsync();
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedores.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Proveedores.AnyAsync(proveedor => proveedor.IdProveedor == id);
        }

        public async Task CrearAsync(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            _context.Proveedores.Update(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Proveedor proveedor)
        {
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
        }
    }
}
