using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ProveedoresEndpoints
    {
        public static void MapProveedoresEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/proveedores").WithTags("Proveedores");
            // obtener todos los proveedores
            group.MapGet("/", async (IProveedoresLogica logica) =>
            {
                var proveedores = await logica.ObtenerTodosAsync();
                return Results.Ok(proveedores);
            });
            // obtener un proveedor por su id
            group.MapGet("/{id:int}", async (int id, IProveedoresLogica logica) =>
            {
                var proveedor = await logica.ObtenerPorIdAsync(id);
                return proveedor is null ? Results.NotFound() : Results.Ok(proveedor);
            });
            // crear un nuevo proveedor
            group.MapPost("/", async (Proveedor proveedor, IProveedoresLogica logica) =>
            {
                var error = await logica.CrearAsync(proveedor);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/proveedores/{proveedor.IdProveedor}", proveedor);
            });
            // actualizar un proveedor existente
            group.MapPut("/{id:int}", async (int id, Proveedor proveedor, IProveedoresLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                proveedor.IdProveedor = id;
                var error = await logica.ActualizarAsync(proveedor);
                return error is null ? Results.Ok(proveedor) : Results.BadRequest(error);
            });
            // eliminar un proveedor
            group.MapDelete("/{id:int}", async (int id, IProveedoresLogica logica) =>
            {
                try
                {
                    var eliminado = await logica.EliminarAsync(id);
                    return eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            });
        }
    }
}
