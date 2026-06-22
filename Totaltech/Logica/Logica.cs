using Microsoft.EntityFrameworkCore;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public class Logica<TEntidad> : ILogica<TEntidad> where TEntidad : class
    {
        protected readonly IRepositorio<TEntidad> Repositorio;

        public Logica(IRepositorio<TEntidad> repositorio)
        {
            Repositorio = repositorio;
        }

        public virtual Task<List<TEntidad>> ObtenerTodosAsync()
        {
            return Repositorio.ObtenerTodosAsync();
        }

        public virtual Task<TEntidad?> ObtenerPorIdAsync(int id)
        {
            return Repositorio.ObtenerPorIdAsync(id);
        }

        public virtual Task CrearAsync(TEntidad entidad)
        {
            return Repositorio.CrearAsync(entidad);
        }

        public virtual Task ActualizarAsync(TEntidad entidad)
        {
            return Repositorio.ActualizarAsync(entidad);
        }

        public virtual Task EliminarAsync(TEntidad entidad)
        {
            return Repositorio.EliminarAsync(entidad);
        }

        public virtual async Task<ResultadoOperacion<TEntidad>> CrearValidadoAsync(TEntidad entidad)
        {
            await Repositorio.CrearAsync(entidad);
            return ResultadoOperacion<TEntidad>.Ok(entidad);
        }

        public virtual async Task<ResultadoOperacion<TEntidad>> ActualizarValidadoAsync(int id, TEntidad entidad)
        {
            var existente = await Repositorio.ExisteAsync(id);

            if (!existente)
            {
                return ResultadoOperacion<TEntidad>.NotFound("No se encontro el registro solicitado.");
            }

            await Repositorio.ActualizarAsync(entidad);
            return ResultadoOperacion<TEntidad>.Ok(entidad);
        }

        public virtual async Task<ResultadoOperacion> EliminarPorIdAsync(int id)
        {
            var entidad = await Repositorio.ObtenerPorIdAsync(id);

            if (entidad is null)
            {
                return ResultadoOperacion.NotFound("No se encontro el registro solicitado.");
            }

            try
            {
                await Repositorio.EliminarAsync(entidad);
                return ResultadoOperacion.Ok();
            }
            catch (DbUpdateException)
            {
                return ResultadoOperacion.Conflict("No se puede eliminar porque el registro tiene datos relacionados.");
            }
        }
    }
}
