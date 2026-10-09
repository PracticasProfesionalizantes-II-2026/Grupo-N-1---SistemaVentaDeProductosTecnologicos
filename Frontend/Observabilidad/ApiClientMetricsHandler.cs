using System.Diagnostics;
using Prometheus;

namespace Frontend.Observabilidad;

public sealed class ApiClientMetricsHandler : DelegatingHandler
{
    private static readonly double[] DurationBuckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];
    private readonly string _client;
    private readonly bool _enabled;
    private readonly Counter _requests;
    private readonly Histogram _duration;

    public ApiClientMetricsHandler(IMetricFactory metrics, string client, bool enabled)
    {
        if (client is not ("TotaltechApi" or "TotaltechSessionApi"))
            throw new ArgumentException("El cliente de métricas debe ser un cliente API registrado.", nameof(client));

        _client = client;
        _enabled = enabled;
        _requests = metrics.CreateCounter("totaltech_api_client_requests_total",
            "Llamadas MVC a la API por cliente, método y resultado.",
            new CounterConfiguration { LabelNames = ["client", "method", "result"] });
        _duration = metrics.CreateHistogram("totaltech_api_client_request_duration_seconds",
            "Duración de la llamada MVC a la API hasta recibir cabeceras, en segundos.",
            new HistogramConfiguration { LabelNames = ["client", "method", "result"], Buckets = DurationBuckets });
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_enabled)
            return await base.SendAsync(request, cancellationToken);

        var method = request.Method.Method.ToUpperInvariant();
        method = method switch
        {
            "GET" or "POST" or "PUT" or "PATCH" or "DELETE" or "HEAD" or "OPTIONS" => method,
            _ => "OTHER"
        };
        var started = Stopwatch.GetTimestamp();
        var result = "error";
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            var status = (int)response.StatusCode;
            result = status is >= 100 and < 600 ? $"http_{status / 100}xx" : "error";
            return response;
        }
        catch (OperationCanceledException)
        {
            result = "cancelled";
            throw;
        }
        catch (HttpRequestException)
        {
            result = "transport_error";
            throw;
        }
        finally
        {
            _requests.WithLabels(_client, method, result).Inc();
            _duration.WithLabels(_client, method, result).Observe(Stopwatch.GetElapsedTime(started).TotalSeconds);
        }
    }
}
