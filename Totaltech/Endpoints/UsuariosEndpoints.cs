using System.Security.Claims;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints;

public static class UsuariosEndpoints
{
    public static void MapUsuariosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/usuarios").WithTags("Usuarios");
        group.MapGet("/", async (IUsuariosLogica logica) =>
            Results.Ok((await logica.ObtenerTodosAsync()).Select(CrearRespuesta)))
            .RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapGet("/{id:int}", async (int id, IUsuariosLogica logica, ClaimsPrincipal actual) =>
        {
            var usuario = await logica.ObtenerPorIdAsync(id);
            return usuario is null || !actual.PuedeAcceder(id)
                ? Results.NotFound() : Results.Ok(CrearRespuesta(usuario));
        }).RequireAuthorization();

        // Contrato administrativo existente; la nueva UI usa el registro público.
        group.MapPost("/", async (UsuarioRequest request, IUsuariosLogica logica) =>
        {
            var usuario = request.ToEntity();
            var error = await logica.CrearAsync(usuario);
            return error is null
                ? Results.Created($"/usuarios/{usuario.IdUsuario}", CrearRespuesta(usuario))
                : error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapPut("/{id:int}", async (
            int id, UsuarioActualizacionRequest request, IUsuariosLogica logica, ClaimsPrincipal actual) =>
            Responder(await logica.ActualizarAsync(id, request, actual.ObtenerIdUsuario()!.Value, actual.EsAdministrador())))
            .RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, IUsuariosLogica logica, ClaimsPrincipal actual) =>
            Responder(await logica.CambiarEstadoAsync(id, false, actual.ObtenerIdUsuario()!.Value), sinContenido: true))
            .RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapPost("/{id:int}/reactivar", async (int id, IUsuariosLogica logica, ClaimsPrincipal actual) =>
            Responder(await logica.CambiarEstadoAsync(id, true, actual.ObtenerIdUsuario()!.Value)))
            .RequireAuthorization(Autorizacion.PoliticaAdministrador);
    }

    private static IResult Responder(ResultadoUsuario resultado, bool sinContenido = false) => resultado.Estado switch
    {
        EstadoOperacionUsuario.NoEncontrado => Results.NotFound(),
        EstadoOperacionUsuario.Invalido => Results.BadRequest(resultado.Error),
        EstadoOperacionUsuario.Conflicto => Results.Conflict(resultado.Error),
        _ => sinContenido ? Results.NoContent() : Results.Ok(CrearRespuesta(resultado.Usuario!))
    };

    public static UsuarioResponse CrearRespuesta(Usuario usuario) => new()
    {
        IdUsuario = usuario.IdUsuario, Nombre = usuario.Nombre, Apellido = usuario.Apellido,
        Email = usuario.Email, Telefono = usuario.Telefono, FechaRegistro = usuario.FechaRegistro,
        Rol = usuario.Rol, Activo = usuario.Activo
    };
}
