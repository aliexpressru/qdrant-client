using Aer.QdrantClient.Http.Models.Responses.Base;
using OpenTelemetry.Trace;

namespace Aer.QdrantClient.Http.Diagnostics.Tracing;

/// <summary>
/// Disposable scope that manages the lifecycle of a tracing span.
/// </summary>
internal sealed class TracingScope : IDisposable
{
    private readonly TelemetrySpan _span;
    private bool _disposed;
    private readonly bool _isDisabled;

    public static TracingScope Disabled { get; } = new TracingScope(isDisabled: true);

    private TracingScope(bool isDisabled)
    {
        _isDisabled = isDisabled;
    }

    internal TracingScope(TelemetrySpan span)
    {
        _span = span ?? throw new ArgumentNullException(nameof(span));
    }

    /// <summary>
    /// Sets the result status on the span.
    /// </summary>
    /// <param name="success">Whether the operation was successful.</param>
    /// <param name="errorMessage">Optional error message if operation failed.</param>
    public void SetResult(bool success, string errorMessage = null)
    {
        if (_disposed || _isDisabled)
        {
            return;
        }

        try
        {
            _span.SetStatus(success ? Status.Ok : Status.Error.WithDescription(errorMessage ?? "Operation failed"));
        }
        catch
        {
            // Tracing should never break the application
            // Silently ignore any exceptions from tracing operations
        }
    }

    /// <summary>
    /// Sets the result status on the span form a result of qdrant operation.
    /// </summary>
    /// <param name="qdrantResponse">The qdrant operation response.</param>
    public void SetResult(QdrantResponseBase qdrantResponse)
    {
        if (_disposed || _isDisabled)
        {
            return;
        }

        try
        {
            _span.SetStatus(qdrantResponse.Status.IsSuccess ? Status.Ok : Status.Error.WithDescription(qdrantResponse.Status.GetErrorMessage() ?? "Operation failed"));
        }
        catch
        {
            // Tracing should never break the application
            // Silently ignore any exceptions from tracing operations
        }
    }

    /// <summary>
    /// Sets an error status on the span based on an exception.
    /// </summary>
    public void SetError(Exception exception)
    {
        if (_disposed || _isDisabled)
        {
            return;
        }

        try
        {
            _span.RecordException(exception);
            _span.SetStatus(Status.Error.WithDescription(exception.Message));
        }
        catch
        {
            // Tracing should never break the application
            // Silently ignore any exceptions from tracing operations
        }
    }

    /// <summary>
    /// Ends the span and disposes the scope.
    /// </summary>
    public void Dispose()
    {
        if (_disposed || _isDisabled)
        {
            return;
        }

        try
        {
            _span.End();
        }
        catch
        {
            // Tracing should never break the application
            // Silently ignore any exceptions from tracing operations
        }
        finally
        {
            _disposed = true;
        }
    }
}
