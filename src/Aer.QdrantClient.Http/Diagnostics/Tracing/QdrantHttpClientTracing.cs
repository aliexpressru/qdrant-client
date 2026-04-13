using System.Diagnostics;
using Aer.QdrantClient.Http.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Aer.QdrantClient.Http.Diagnostics.Tracing;

internal static class QdrantHttpClientTracing
{
    private const string DbSystemValue = "qdrant";
    private const string SpanNamePrefix = "qdrant.http ";

    // Attribute names
    private const string DbSystemAttribute = "db.system";
    private const string DbOperationNameAttribute = "db.operation.name";

    //private const string ServerAddressAttribute = "server.address";

    /// <summary>
    /// Creates a tracing scope for a qdrant request.
    /// Returns null if tracing is not enabled.
    /// </summary>
    public static TracingScope CreateRequestScope(
        Tracer tracer,
        string methodName,
        bool enableTracing,
        ILogger logger = null,
        TracingOptions tracingOptions = null
    )
    {
        if (!ShouldEnableTracing(tracer, tracingOptions, enableTracing))
        {
            return TracingScope.Disabled;
        }

        try
        {
            // Create span following OpenTelemetry semantic conventions
            var span = tracer
                .StartActiveSpan($"{SpanNamePrefix}{methodName}", SpanKind.Client, Tracer.CurrentSpan)
                // Database semantic conventions
                .SetAttribute(DbSystemAttribute, DbSystemValue)
                .SetAttribute(DbOperationNameAttribute, methodName);
            // Network semantic conventions
            //.SetAttribute(ServerAddressAttribute, serverAddress ?? "N/A");

            return new TracingScope(span);
        }
        catch (Exception ex)
        {
            // Tracing should never break the application
            // Common cases:
            // - ObjectDisposedException: Activity source disposed when request completed
            // - NullReferenceException: HTTP context disposed during fire-and-forget operations
            logger?.LogWarning(
                ex,
                "Failed to create tracing scope for qdrant method {MethodName}. Tracing will be disabled for this operation.",
                methodName
            );

            return TracingScope.Disabled;
        }
    }

    /// <summary>
    /// Determines whether tracing should be enabled based on global EnableTracing configuration,
    /// current activity context, and tracing options.
    /// </summary>
    private static bool ShouldEnableTracing(Tracer tracer, TracingOptions tracingOptions, bool enableTracing)
    {
        // Tracer is not registered
        if (tracer is null)
        {
            return false;
        }
        // Global EnableTracing = false always disables tracing, regardless of Tracer availability
        if (!enableTracing)
        {
            return false;
        }

        // No parent activity context - nothing to trace to
        if (Activity.Current == null)
        {
            return false;
        }

        // Manual per-operation disable
        if (tracingOptions?.ManualDisableTracing == true)
        {
            return false;
        }

        return true;
    }
}
