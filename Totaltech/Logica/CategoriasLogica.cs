using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface ICategoriasLogica : ILogica<Categoria>
    {
    }

    public class CategoriasLogica : Logica<Categoria>, ICategoriasLogica
    {
        public CategoriasLogica(ICategoriasRepositorio repositorio) : base(repositorio)
        {
        }

        public override Task<ResultadoOperacion<Categoria>> CrearValidadoAsync(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return Task.FromResult(ResultadoOperacion<Categoria>.BadRequest("El nombre de la categoria es obligatorio."));
            }

            return base.CrearValidadoAsync(categoria);
        }

        public override async Task<ResultadoOperacion<Categoria>> ActualizarValidadoAsync(int id, Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return ResultadoOperacion<Categoria>.BadRequest("El nombre de la categoria es obligatorio.");
            }

            return await base.ActualizarValidadoAsync(id, categoria);
        }
    }
}
