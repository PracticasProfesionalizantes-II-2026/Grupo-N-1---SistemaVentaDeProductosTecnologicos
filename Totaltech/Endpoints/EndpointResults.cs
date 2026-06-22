using Microsoft.EntityFrameworkCore;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    internal static class EndpointResults
    {
        public static IResult FromResult<T>(ResultadoOperacion<T> resultado, Func<T, IResult> onSuccess)
        {
            if (resultado.Exitoso && resultado.Datos is not null)
            {
                return onSuccess(resultado.Datos);
            }

            return resultado.TipoError switch
            {
                TipoErrorOperacion.NotFound => Results.NotFound(resultado.Error),
                TipoErrorOperacion.Conflict => Results.Conflict(resultado.Error),
                _ => Results.BadRequest(resultado.Error)
            };
        }

        public static IResult FromResult(ResultadoOperacion resultado, Func<IResult> onSuccess)
        {
            if (resultado.Exitoso)
            {
                return onSuccess();
            }

            return resultado.TipoError switch
            {
                TipoErrorOperacion.NotFound => Results.NotFound(resultado.Error),
                TipoErrorOperacion.Conflict => Results.Conflict(resultado.Error),
                _ => Results.BadRequest(resultado.Error)
            };
        }

        public static async Task<IResult> HandleDbUpdateAsync(Func<Task<IResult>> action)
        {
            try
            {
                return await action();
            }
            catch (DbUpdateException)
            {
                return Results.Conflict("La operacion no se pudo completar por restricciones de datos relacionados.");
            }
        }
    }
}
