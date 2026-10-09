using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using Totaltech.Observabilidad;

namespace Totaltech.IntegrationTests.Observabilidad;

public sealed class SqlDependencyWorkerTests
{
    [Fact]
    public async Task ErrorCreandoScopeNoDetieneElWorkerNiElArranque()
    {
        var scopes = new ScopeFactoryFallida();
        var logger = new LoggerPrueba();
        var registry = Metrics.NewCustomRegistry();
        using var worker = new SqlDependencyWorker(scopes, Metrics.WithCustomRegistry(registry), logger);

        await worker.StartAsync(CancellationToken.None);
        await logger.Advertencia.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(1, scopes.Intentos);
        Assert.False(worker.ExecuteTask!.IsCompleted);
        await worker.StopAsync(CancellationToken.None);
        await worker.ExecuteTask;
    }

    private sealed class ScopeFactoryFallida : IServiceScopeFactory
    {
        public int Intentos { get; private set; }
        public IServiceScope CreateScope()
        {
            Intentos++;
            throw new InvalidOperationException("Fallo de scope controlado.");
        }
    }

    private sealed class LoggerPrueba : ILogger<SqlDependencyWorker>
    {
        public TaskCompletionSource Advertencia { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning) Advertencia.TrySetResult();
        }
    }
}
