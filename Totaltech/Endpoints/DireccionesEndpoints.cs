using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DireccionesEndpoints
    {
        public static void MapDireccionesEndpoints(this WebApplication app)
        {
            app.MapCrud<Direccion, IDireccionesLogica>("/direcciones", "Direcciones", direccion => direccion.IdDireccion);
        }
    }
}
