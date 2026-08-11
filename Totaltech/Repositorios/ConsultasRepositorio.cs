using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IConsultasRepositorio
    {
        Task<List<Consulta>> ObtenerTodosAsync();
        Task<Consulta?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Consulta consulta);
        Task ActualizarAsync(Consulta consulta);
        Task EliminarAsync(Consulta consulta);
        Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario);
    }

    public class ConsultasRepositorio : IConsultasRepositorio
    {
        private readonly TotaltechDbContext _context;

        public ConsultasRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Consulta>> ObtenerTodosAsync()
        {
            return await _context.Consultas.ToListAsync();
        }

        public async Task<Consulta?> ObtenerPorIdAsync(int id)
        {
            return await _context.Consultas.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Consultas.AnyAsync(consulta => consulta.IdConsulta == id);
        }

        public async Task CrearAsync(Consulta consulta)
        {
            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Consulta consulta)
        {
            _context.Consultas.Update(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Consulta consulta)
        {
            _context.Consultas.Remove(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return await _context.Consultas
                .Where(consulta => consulta.IdUsuario == idUsuario)
                .ToListAsync();
        }
    }
}
