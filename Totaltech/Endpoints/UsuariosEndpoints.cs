using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/usuarios").WithTags("Usuarios");

            group.MapGet("/", async (IUsuariosLogica logica) =>
            {
                var usuarios = await logica.ObtenerTodosAsync();
                return Results.Ok(usuarios.Select(CrearRespuesta));
            });

            group.MapGet("/{id:int}", async (int id, IUsuariosLogica logica) =>
            {
                var usuario = await logica.ObtenerPorIdAsync(id);
                return usuario is null ? Results.NotFound() : Results.Ok(CrearRespuesta(usuario));
            });

            group.MapPost("/", async (Usuario usuario, IUsuariosLogica logica) =>
            {
                var error = await logica.CrearAsync(usuario);
                if (error is not null)
                {
                    return error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
                }

                return Results.Created($"/usuarios/{usuario.IdUsuario}", CrearRespuesta(usuario));
            });

            group.MapPut("/{id:int}", async (int id, Usuario usuario, IUsuariosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var error = await logica.ActualizarAsync(id, usuario);
                if (error is not null)
                {
                    return error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
                }

                var actualizado = await logica.ObtenerPorIdAsync(id);
                return Results.Ok(CrearRespuesta(actualizado!));
            });

            group.MapDelete("/{id:int}", async (int id, IUsuariosLogica logica) =>
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

        private static UsuarioResponse CrearRespuesta(Usuario usuario)
        {
            return new UsuarioResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FechaRegistro = usuario.FechaRegistro,
                Rol = usuario.Rol
            };
        }
    }
}
