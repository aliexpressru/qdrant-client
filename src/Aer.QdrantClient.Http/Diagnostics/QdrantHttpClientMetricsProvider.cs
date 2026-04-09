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

    private const string EndpointLabel = "endpoint";
    private const string IsSuccessfulLabel = "is_successful";

    public QdrantHttpClientMetricsProvider(
        IMeterFactory meterFactory)
    {
        if (meterFactory is null)
        {
            throw new ArgumentNullException(nameof(meterFactory));
        }

        var meter = meterFactory.Create(MeterName);

        _requestDurationSeconds = meter.CreateHistogram<double>(
            name: RequestDurationSecondsMetricName,
            unit: null,
            description: "Qdrant request duration in seconds");

        _requestsTotal = meter.CreateCounter<int>(
            name: RequestsTotalMetricName,
            unit: null,
            description: "Number of total executed qdrant requests");
    }

    /// <summary>
    /// Observes the duration of a command.
    /// </summary>
    /// <param name="endpoint">Name of an endpoint.</param>
    /// <param name="durationSeconds">Duration of a request in seconds.</param>
    public void ObserveRequestDurationSeconds(string endpoint, double durationSeconds)
    {
        _requestDurationSeconds?.Record(
            durationSeconds,
            new KeyValuePair<string, object>(EndpointLabel, endpoint));
    }

    /// <summary>
    /// Observes an executed request.
    /// </summary>
    /// <param name="endpoint">Name of an endpoint.</param>
    /// <param name="isSuccessful">Request executed successfully or not. 0 and 1 as values.</param>
    public void ObserveExecutedRequest(string endpoint, string isSuccessful)
    {
        _requestsTotal?.Add(
            1,
            new KeyValuePair<string, object>[]
            {
                new(EndpointLabel, endpoint), new(IsSuccessfulLabel, isSuccessful)
            });
    }
}
