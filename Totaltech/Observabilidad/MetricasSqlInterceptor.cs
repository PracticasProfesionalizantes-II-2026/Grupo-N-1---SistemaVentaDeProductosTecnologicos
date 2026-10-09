using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prometheus;

namespace Totaltech.Observabilidad;

public sealed class MetricasSqlInterceptor : DbCommandInterceptor
{
    private readonly Counter? _comandos;
    private readonly Histogram? _duracion;

    public MetricasSqlInterceptor(IMetricFactory factory, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Observability:Enabled")) return;
        _comandos = factory.CreateCounter("totaltech_db_commands_total",
            "Comandos EF Core ejecutados, incluidos los intentos de reintento.", ["execution", "result"]);
        _duracion = factory.CreateHistogram("totaltech_db_command_duration_seconds",
            "Duración de ejecución de comandos EF Core hasta recibir el resultado.",
            new HistogramConfiguration
            {
                LabelNames = ["execution", "result"],
                Buckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
            });
    }

    private void Registrar(CommandEndEventData data, string resultado)
    {
        if (_comandos is null) return;
        var ejecucion = data.ExecuteMethod switch
        {
            DbCommandMethod.ExecuteReader => "reader",
            DbCommandMethod.ExecuteScalar => "scalar",
            DbCommandMethod.ExecuteNonQuery => "nonquery",
            _ => "other"
        };
        _comandos.WithLabels(ejecucion, resultado).Inc();
        _duracion!.WithLabels(ejecucion, resultado).Observe(data.Duration.TotalSeconds);
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Registrar(eventData, "success");
        return result;
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Registrar(eventData, "success");
        return result;
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Registrar(eventData, "success");
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        DbDataReader result, CancellationToken cancellationToken = default)
    {
        Registrar(eventData, "success");
        return ValueTask.FromResult(result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        object? result, CancellationToken cancellationToken = default)
    {
        Registrar(eventData, "success");
        return ValueTask.FromResult(result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
        int result, CancellationToken cancellationToken = default)
    {
        Registrar(eventData, "success");
        return ValueTask.FromResult(result);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData) => Registrar(eventData, "error");
    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        // Algunos proveedores devuelven su excepción propia al cancelar. La
        // cancelación del token evita clasificar esos comandos como fallos SQL.
        Registrar(eventData, cancellationToken.IsCancellationRequested || eventData.Exception is OperationCanceledException
            ? "cancelled" : "error");
        return Task.CompletedTask;
    }

    public override void CommandCanceled(DbCommand command, CommandEndEventData eventData) => Registrar(eventData, "cancelled");
    public override Task CommandCanceledAsync(DbCommand command, CommandEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Registrar(eventData, "cancelled");
        return Task.CompletedTask;
    }
}
