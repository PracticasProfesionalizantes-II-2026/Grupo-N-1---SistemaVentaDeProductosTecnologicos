using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

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
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
            // obtener un usuario por su id
            group.MapGet("/{id:int}", async (int id, IUsuariosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var usuario = await logica.ObtenerPorIdAsync(id);
                return usuario is null || !usuarioActual.PuedeAcceder(usuario.IdUsuario)
                    ? Results.NotFound()
                    : Results.Ok(CrearRespuesta(usuario));
            }).RequireAuthorization();
            // crear un nuevo usuario
            group.MapPost("/", async (UsuarioRequest request, IUsuariosLogica logica) =>
            {
                var usuario = request.ToEntity();
                var error = await logica.CrearAsync(usuario);
                if (error is not null)
                {
                    return error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
                }

                return Results.Created($"/usuarios/{usuario.IdUsuario}", CrearRespuesta(usuario));
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
            // actualizar un usuario existente
            group.MapPut("/{id:int}", async (int id, UsuarioRequest request, IUsuariosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null || !usuarioActual.PuedeAcceder(existente.IdUsuario))
                {
                    return Results.NotFound();
                }

                var usuario = request.ToEntity();
                if (!usuarioActual.EsAdministrador())
                {
                    usuario.Rol = existente.Rol;
                }

                var error = await logica.ActualizarAsync(id, usuario);
                if (error is not null)
                {
                    return error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
                }

                var actualizado = await logica.ObtenerPorIdAsync(id);
                return Results.Ok(CrearRespuesta(actualizado!));
            }).RequireAuthorization();

            // eliminar un usuario
            group.MapDelete("/{id:int}", async (int id, IUsuariosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var usuario = await logica.ObtenerPorIdAsync(id);
                if (usuario is null || !usuarioActual.PuedeAcceder(usuario.IdUsuario))
                {
                    return Results.NotFound();
                }

                try
                {
                    var eliminado = await logica.EliminarAsync(id);
                    return eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            }).RequireAuthorization();
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
