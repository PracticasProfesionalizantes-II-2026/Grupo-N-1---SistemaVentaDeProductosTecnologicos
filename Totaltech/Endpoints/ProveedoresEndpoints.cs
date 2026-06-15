using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ProveedoresEndpoints
    {
        public static void MapProveedoresEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/proveedores").WithTags("Proveedores");

            group.MapGet("/", async (IProveedoresLogica logica) =>
            {
                var proveedores = await logica.ObtenerTodosAsync();
                return Results.Ok(proveedores);
            });

            group.MapGet("/{id:int}", async (int id, IProveedoresLogica logica) =>
            {
                var proveedor = await logica.ObtenerPorIdAsync(id);
                return proveedor is null ? Results.NotFound() : Results.Ok(proveedor);
            });

            group.MapPost("/", async (Proveedor proveedor, IProveedoresLogica logica) =>
            {
                await logica.CrearAsync(proveedor);
                return Results.Created($"/proveedores/{proveedor.IdProveedor}", proveedor);
            });

            group.MapPut("/{id:int}", async (int id, Proveedor proveedor, IProveedoresLogica logica) =>
            {
                if (id != proveedor.IdProveedor)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(proveedor);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IProveedoresLogica logica) =>
            {
                var proveedor = await logica.ObtenerPorIdAsync(id);
                if (proveedor is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(proveedor);
                return Results.NoContent();
            });
        }
    }
}
