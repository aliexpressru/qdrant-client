using Aer.QdrantClient.Http.Configuration;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Aer.QdrantClient.Http.Diagnostics.Listeners;

internal class MetricsQdrantHttpClientDiagnosticListener
{
    private readonly QdrantHttpClientMetricsProvider _metricsProvider;
    private readonly QdrantClientSettings _clientSettings;

    public MetricsQdrantHttpClientDiagnosticListener(
        QdrantHttpClientMetricsProvider metricsProvider,
        QdrantClientSettings qdrantClientSettings
    )
    {
        _metricsProvider = metricsProvider;
        _clientSettings = qdrantClientSettings;
    }

    [DiagnosticName(QdrantHttpClientDiagnosticSource.RequestDurationDiagnosticName)]
    public void ObserveRequestDuration(string collectionName, string methodName, double duration, string clusterName)
    {
        if (_clientSettings.DisableMetrics)
        {
            return;
        }

        _metricsProvider.ObserveRequestDurationSeconds(collectionName, methodName, duration, clusterName);
    }

    [DiagnosticName(QdrantHttpClientDiagnosticSource.RequestsTotalDiagnosticName)]
    public void ObserveExecutedRequest(string collectionName, string methodName, string isSuccessful, string clusterName)
    {
        if (_clientSettings.DisableMetrics)
        {
            return;
        }

        _metricsProvider.ObserveExecutedRequest(collectionName, methodName, isSuccessful, clusterName);
    }
}
