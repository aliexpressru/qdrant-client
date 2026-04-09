using Aer.QdrantClient.Http.Configuration;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Options;

namespace Aer.QdrantClient.Http.Diagnostics.Listeners;

internal class MetricsQdrantHttpClientDiagnosticListener
{
    private readonly QdrantHttpClientMetricsProvider _metricsProvider;
    private readonly QdrantClientSettings _clientSettings;

    public MetricsQdrantHttpClientDiagnosticListener(
        QdrantHttpClientMetricsProvider metricsProvider,
        IOptions<QdrantClientSettings> config)
    {
        _metricsProvider = metricsProvider;
        _clientSettings = config.Value;
    }

    [DiagnosticName(QdrantHttpClientDiagnosticSource.RequestDurationDiagnosticName)]
    public void ObserveRequestDuration(string endpoint, double duration)
    {
        if (_clientSettings.DisableMetrics)
        {
            return;
        }

        _metricsProvider.ObserveRequestDurationSeconds(endpoint, duration);
    }

    [DiagnosticName(QdrantHttpClientDiagnosticSource.RequestsTotalDiagnosticName)]
    public void ObserveExecutedRequest(string endpoint, string isSuccessful)
    {
        if (_clientSettings.DisableMetrics)
        {
            return;
        }

        _metricsProvider.ObserveExecutedRequest(endpoint, isSuccessful);
    }
}
