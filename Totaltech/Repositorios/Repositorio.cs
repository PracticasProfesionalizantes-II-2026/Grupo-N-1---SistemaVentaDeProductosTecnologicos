using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;

namespace Totaltech.Repositorios
{
    public class Repositorio<TEntidad> : IRepositorio<TEntidad> where TEntidad : class
    {
        protected readonly TotaltechDbContext Context;

        public Repositorio(TotaltechDbContext context)
        {
            Context = context;
        }

        public async Task<List<TEntidad>> ObtenerTodosAsync()
        {
            return await Context.Set<TEntidad>().ToListAsync();
        }

        public async Task<TEntidad?> ObtenerPorIdAsync(int id)
        {
            return await Context.Set<TEntidad>().FindAsync(id);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            var tipoEntidad = Context.Model.FindEntityType(typeof(TEntidad));
            var clavePrimaria = tipoEntidad?.FindPrimaryKey()?.Properties.SingleOrDefault();

            if (clavePrimaria is null)
            {
                return false;
            }

            return await Context.Set<TEntidad>()
                .AnyAsync(entidad => EF.Property<int>(entidad, clavePrimaria.Name) == id);
        }

        public async Task CrearAsync(TEntidad entidad)
        {
            Context.Set<TEntidad>().Add(entidad);
            await Context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(TEntidad entidad)
        {
            Context.Set<TEntidad>().Update(entidad);
            await Context.SaveChangesAsync();
        }

        public async Task EliminarAsync(TEntidad entidad)
        {
            Context.Set<TEntidad>().Remove(entidad);
            await Context.SaveChangesAsync();
        }
    }
}
