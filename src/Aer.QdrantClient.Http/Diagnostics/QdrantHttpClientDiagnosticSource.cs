using System.Diagnostics;

namespace Aer.QdrantClient.Http.Diagnostics;

/// <summary>
/// The default qdrant http client diagnostic listener.
/// </summary>
public class QdrantHttpClientDiagnosticSource : DiagnosticListener
{
    private const string SourceName = "Aer.Diagnostics.QdrantHttp";

    /// <summary>
    /// The name of the qdrant request duration diagnostic.
    /// </summary>
    public const string RequestDurationDiagnosticName = SourceName + ".RequestDuration";

    /// <summary>
    /// The name if the qdrant total requests count diagnostic.
    /// </summary>
    public const string RequestsTotalDiagnosticName = SourceName + ".RequestsTotal";

    /// <summary>
    /// Returns a default instance of <see cref="QdrantHttpClientDiagnosticSource"/>.
    /// </summary>
    public static QdrantHttpClientDiagnosticSource Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="QdrantHttpClientDiagnosticSource"/>.
    /// </summary>
    public QdrantHttpClientDiagnosticSource() : base(SourceName)
    {
    }
}
