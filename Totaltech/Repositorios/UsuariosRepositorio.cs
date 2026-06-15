using Totaltech.Datos;
using Totaltech.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Totaltech.Repositorios
{
    public interface IUsuariosRepositorio : IRepositorio<Usuario>
    {
        Task<Usuario?> ObtenerPorEmailAsync(string email);
    }

    public class UsuariosRepositorio : Repositorio<Usuario>, IUsuariosRepositorio
    {
        public UsuariosRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            return await Context.Usuarios
                .FirstOrDefaultAsync(usuario => usuario.Email == email);
        }
    }
}
