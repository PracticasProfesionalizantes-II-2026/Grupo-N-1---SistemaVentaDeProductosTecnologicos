using System.Diagnostics;
using Prometheus;
using Totaltech.Logica.DTOs;

namespace Totaltech.Observabilidad;

public sealed class MetricasNegocio
{
    private readonly Counter? _autenticacion;
    private readonly Counter? _checkout;
    private readonly Histogram? _duracionCheckout;
    private readonly Counter? _usuarios;

    public MetricasNegocio(IMetricFactory factory, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Observability:Enabled")) return;

        _autenticacion = factory.CreateCounter("totaltech_auth_operations_total",
            "Operaciones de autenticación por resultado final.", ["operation", "result"]);
        _checkout = factory.CreateCounter("totaltech_checkout_operations_total",
            "Confirmaciones del carrito por resultado final.", ["result"]);
        _duracionCheckout = factory.CreateHistogram("totaltech_checkout_duration_seconds",
            "Duración exterior de la confirmación del carrito, incluidos sus reintentos.",
            new HistogramConfiguration
            {
                LabelNames = ["result"],
                Buckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
            });
        _usuarios = factory.CreateCounter("totaltech_user_operations_total",
            "Operaciones de gestión de usuarios por resultado final, incluidas las repetidas sin cambios.",
            ["operation", "result"]);
    }

    public void RegistrarAutenticacion(string operacion, string resultado) =>
        _autenticacion?.WithLabels(operacion, resultado).Inc();

    public async Task<ConfirmarCarritoResultado> MedirCheckoutAsync(Func<Task<ConfirmarCarritoResultado>> operacion)
    {
        if (_checkout is null) return await operacion();
        var inicio = Stopwatch.GetTimestamp();
        var etiqueta = "error";
        try
        {
            var resultado = await operacion();
            etiqueta = resultado.Estado switch
            {
                EstadoConfirmacionCarrito.Creado => "created",
                EstadoConfirmacionCarrito.Repetido => "repeated",
                EstadoConfirmacionCarrito.Invalido => "invalid",
                EstadoConfirmacionCarrito.NoEncontrado => "not_found",
                EstadoConfirmacionCarrito.Conflicto => "conflict",
                _ => "error"
            };
            return resultado;
        }
        catch (OperationCanceledException)
        {
            etiqueta = "cancelled";
            throw;
        }
        finally
        {
            _checkout.WithLabels(etiqueta).Inc();
            _duracionCheckout!.WithLabels(etiqueta).Observe(Stopwatch.GetElapsedTime(inicio).TotalSeconds);
        }
    }

    public async Task<ResultadoUsuario> MedirUsuarioAsync(string nombre, Func<Task<ResultadoUsuario>> operacion)
    {
        if (_usuarios is null) return await operacion();
        var etiqueta = "error";
        try
        {
            var resultado = await operacion();
            etiqueta = resultado.Estado switch
            {
                EstadoOperacionUsuario.Exito => "success",
                EstadoOperacionUsuario.Invalido => "invalid",
                EstadoOperacionUsuario.NoEncontrado => "not_found",
                EstadoOperacionUsuario.Conflicto => "conflict",
                _ => "error"
            };
            return resultado;
        }
        catch (OperationCanceledException)
        {
            etiqueta = "cancelled";
            throw;
        }
        finally { _usuarios.WithLabels(nombre, etiqueta).Inc(); }
    }
}
