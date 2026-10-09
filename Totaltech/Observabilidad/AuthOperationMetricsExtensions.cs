namespace Totaltech.Observabilidad;

public static class AuthOperationMetricsExtensions
{
    public static IApplicationBuilder UseAuthOperationMetrics(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                await next(context);
                return;
            }

            // El cuerpo inválido puede fallar antes de invocar el endpoint. Medir
            // aquí incluye ese 400 sin leer el cuerpo ni duplicar la operación.
            var ruta = context.Request.Path.Value?.TrimEnd('/');
            var operacion = ruta?.ToLowerInvariant() switch
            {
                "/auth/login" => "login",
                "/auth/registro" => "register",
                _ => null
            };
            if (operacion is null)
            {
                await next(context);
                return;
            }

            var metricas = context.RequestServices.GetRequiredService<MetricasNegocio>();
            var resultado = "error";
            try
            {
                await next(context);
                resultado = context.Response.StatusCode switch
                {
                    200 or 201 => "success",
                    400 => "invalid",
                    401 => "invalid_credentials",
                    403 => "inactive",
                    404 => "not_found",
                    409 => "conflict",
                    _ => "error"
                };
            }
            catch (OperationCanceledException)
            {
                resultado = "cancelled";
                throw;
            }
            catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status400BadRequest)
            {
                resultado = "invalid";
                throw;
            }
            finally { metricas.RegistrarAutenticacion(operacion, resultado); }
        });
}
