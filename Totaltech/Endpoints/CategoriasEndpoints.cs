using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this WebApplication app)
        {
            app.MapCrud<Categoria, ICategoriasLogica>("/categorias", "Categorias", categoria => categoria.IdCategoria);
        }
    }
}
