using System.Reflection;

namespace Aer.QdrantClient.Http.Diagnostics;

/// <summary>
/// Contains constants used in metrics and tracing infrastructure registration.
/// </summary>
public class QdrantHttpClientDiagnosticConstants
{
    /// <summary>
    /// The name of the meter that writes out this client metrics.
    /// </summary>
    public static string MetricsMeterName { get; } = "Aer.QdrantClient.Http.Metrics";

    /// <summary>
    /// The name of the tracing activity source.
    /// </summary>
    public static string TracingActivitySourceName { get; } = "Aer.QdrantClient.Http";

    /// <summary>
    /// The name of the tracing activity service.
    /// </summary>
    public static string TracingActivityServiceName { get; } = Assembly.GetEntryAssembly()?.GetName().Name ?? TracingActivitySourceName;
}
