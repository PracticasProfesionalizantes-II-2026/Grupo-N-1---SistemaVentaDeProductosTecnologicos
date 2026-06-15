using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IConsultasRepositorio : IRepositorio<Consulta>
    {
        Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario);
    }

    public class ConsultasRepositorio : Repositorio<Consulta>, IConsultasRepositorio
    {
        public ConsultasRepositorio(TotaltechDbContext context) : base(context)
        {
        }

        public async Task<List<Consulta>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return await Context.Consultas
                .Where(consulta => consulta.IdUsuario == idUsuario)
                .ToListAsync();
        }
    }
}
