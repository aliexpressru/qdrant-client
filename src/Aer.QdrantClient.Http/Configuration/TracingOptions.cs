namespace Aer.QdrantClient.Http.Configuration;

/// <summary>
/// Options for controlling tracing behavior of qdrant operations.
/// </summary>
public class TracingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether tracing should be manually disabled for this operation.
    /// When set to <c>true</c>, no tracing spans will be created even if tracing is enabled globally.
    /// Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// Note that disabling tracing for qdrant operation does not disable it
    /// for underlying http client tracing which is instrumented by runtime and is out of our control.
    /// </remarks>
    public bool ManualDisableTracing { get; set; }

    /// <summary>
    /// Gets a singleton instance with tracing disabled.
    /// </summary>
    public static TracingOptions Disabled { get; } = new() { ManualDisableTracing = true };

    /// <summary>
    /// Gets a singleton instance with tracing enabled.
    /// </summary>
    public static TracingOptions Enabled { get; } = new() { ManualDisableTracing = false };
}
