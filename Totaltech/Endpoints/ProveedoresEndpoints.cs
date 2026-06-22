using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ProveedoresEndpoints
    {
        public static void MapProveedoresEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/proveedores").WithTags("Proveedores");

            group.MapGet("/", async (IProveedoresLogica logica) =>
            {
                var proveedores = await logica.ObtenerTodosAsync();
                return Results.Ok(proveedores.Select(proveedor => proveedor.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IProveedoresLogica logica) =>
            {
                var proveedor = await logica.ObtenerPorIdAsync(id);
                return proveedor is null ? Results.NotFound() : Results.Ok(proveedor.ToResponse());
            });

            group.MapPost("/", async (CrearProveedorRequest request, IProveedoresLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, proveedor => Results.Created($"/proveedores/{proveedor.IdProveedor}", proveedor.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarProveedorRequest request, IProveedoresLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, proveedor => Results.Ok(proveedor.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IProveedoresLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });
        }
    }
}
