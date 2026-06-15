using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class PagosEndpoints
    {
        public static void MapPagosEndpoints(this WebApplication app)
        {
            var group = app.MapCrud<Pago, IPagosLogica>("/pagos", "Pagos", pago => pago.IdPago);

            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPagoRequest request, IPagosLogica logica) =>
            {
                var actualizado = await logica.ActualizarEstadoAsync(id, request.Estado);
                return actualizado ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
