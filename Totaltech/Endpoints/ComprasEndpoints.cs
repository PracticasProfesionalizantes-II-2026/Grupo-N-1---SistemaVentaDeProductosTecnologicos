using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ComprasEndpoints
    {
        public static void MapComprasEndpoints(this WebApplication app)
        {
            app.MapCrud<Compra, IComprasLogica>("/compras", "Compras", compra => compra.IdCompra);
        }
    }
}
