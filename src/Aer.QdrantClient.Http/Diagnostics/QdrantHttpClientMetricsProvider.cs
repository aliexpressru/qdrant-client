using System.Diagnostics.Metrics;

namespace Aer.QdrantClient.Http.Diagnostics;

internal class QdrantHttpClientMetricsProvider
{
    public static readonly string MeterName = "Aer.QdrantClient.Http.Metrics";

    // open telemetry metrics
    private readonly Histogram<double> _requestDurationSeconds;
    private readonly Counter<int> _requestsTotal;

    private const string RequestDurationSecondsMetricName = "qdrant_client_request_duration_seconds";
    private const string RequestsTotalMetricName = "qdrant_client_requests_total";

    private const string CollectionLabel = "collection";
    private const string ClusterLabel = "cluster";
    private const string MethodLabel = "method";
    private const string IsSuccessfulLabel = "is_successful";

    public QdrantHttpClientMetricsProvider(IMeterFactory meterFactory)
    {
        if (meterFactory is null)
        {
            throw new ArgumentNullException(nameof(meterFactory));
        }

        var meter = meterFactory.Create(MeterName);

        _requestDurationSeconds = meter.CreateHistogram<double>(
            name: RequestDurationSecondsMetricName,
            unit: null,
            description: "Qdrant request duration in seconds"
        );

        _requestsTotal = meter.CreateCounter<int>(
            name: RequestsTotalMetricName,
            unit: null,
            description: "Number of total executed qdrant requests"
        );
    }

    /// <summary>
    /// Observes the duration of a command.
    /// </summary>
    /// <param name="collectionName">The name of the collection. Might be null if method is not collection-specific.</param>
    /// <param name="methodName">Name of a qdrant http client method.</param>
    /// <param name="clusterName">The name of the qdrant cluster. Might be null if no cluster selected.</param>
    /// <param name="durationSeconds">Duration of a request in seconds.</param>
    public void ObserveRequestDurationSeconds(
        string collectionName,
        string methodName,
        double durationSeconds,
        string clusterName
    )
    {
        if (methodName is null)
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        _requestDurationSeconds?.Record(
            durationSeconds,
            new KeyValuePair<string, object>[]
            {
                new(CollectionLabel, collectionName ?? "N/A"),
                new(MethodLabel, methodName),
                new(ClusterLabel, clusterName ?? "N/A"),
            }
        );
    }

    /// <summary>
    /// Observes an executed request.
    /// </summary>
    /// <param name="collectionName">The name of the collection. Might be null if method is not collection-specific.</param>
    /// <param name="methodName">Name of a qdrant http client method.</param>
    /// <param name="clusterName">The name of the qdrant cluster. Might be null if no cluster selected.</param>
    /// <param name="isSuccessful">Request executed successfully or not. 0 and 1 as values.</param>
    public void ObserveExecutedRequest(string collectionName, string methodName, string isSuccessful, string clusterName)
    {
        if (methodName is null)
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        _requestsTotal?.Add(
            1,
            new KeyValuePair<string, object>[]
            {
                new(CollectionLabel, collectionName ?? "N/A"),
                new(MethodLabel, methodName),
                new(ClusterLabel, clusterName ?? "N/A"),
                new(IsSuccessfulLabel, isSuccessful),
            }
        );
    }
}
