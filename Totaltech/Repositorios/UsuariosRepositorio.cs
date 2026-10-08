using System.Data;
using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IUsuariosRepositorio
    {
        Task<List<Usuario>> ObtenerTodosAsync();
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task CrearAsync(Usuario usuario);
        Task ActualizarContrasenaAsync(Usuario usuario);
        Task<int> ContarAdministradoresActivosAsync();
        Task GuardarCambioAsync(Usuario usuario, AuditoriaUsuario auditoria);
        Task<T> EjecutarTransaccionAsync<T>(Func<Task<T>> operacion);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
    }

    public class UsuariosRepositorio : IUsuariosRepositorio
    {
        private readonly TotaltechDbContext _context;

        public UsuariosRepositorio(TotaltechDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios.OrderBy(u => u.IdUsuario).ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Usuarios.AnyAsync(usuario => usuario.IdUsuario == id);
        }

        public async Task CrearAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarContrasenaAsync(Usuario usuario)
        {
            // Un rehash durante login no puede sobrescribir un rol o baja concurrente.
            _context.Entry(usuario).Property(u => u.Contrasena).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public Task<int> ContarAdministradoresActivosAsync() =>
            _context.Usuarios.CountAsync(u => u.Activo && u.Rol == RolUsuario.Administrador);

        public async Task GuardarCambioAsync(Usuario usuario, AuditoriaUsuario auditoria)
        {
            _context.Usuarios.Update(usuario);
            _context.AuditoriaUsuarios.Add(auditoria);
            await _context.SaveChangesAsync();
        }

        public async Task<T> EjecutarTransaccionAsync<T>(Func<Task<T>> operacion)
        {
            if (!_context.Database.IsRelational()) return await operacion();

            // Releer en cada intento evita estado anterior a un rollback.
            // Serializable protege el conteo del último administrador activo.
            return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var transaccion = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var resultado = await operacion();
                await transaccion.CommitAsync();
                return resultado;
            });
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            var emailNormalizado = email.Trim().ToUpperInvariant();

            return await _context.Usuarios
                .FirstOrDefaultAsync(usuario =>
                    usuario.Email.Trim().ToUpper() == emailNormalizado);
        }
    }
}
