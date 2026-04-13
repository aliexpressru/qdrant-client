using System.Diagnostics;

namespace Aer.QdrantClient.Http.Diagnostics.Helpers;

internal class DiagnosticTimer : IDisposable
{
    private Stopwatch _stopwatch;
    private string _methodName;
    private string _collectionName;
    private string _clusterName;

    private bool _isSuccessful = false;

    // This instance gets returned if diagnostic is disabled.
    private static readonly DiagnosticTimer _disabledTimer = new();

    public static DiagnosticTimer StartNew(string collectionName, string methodName, string clusterName)
    {
        if (!QdrantHttpClientDiagnosticSource.Instance.IsEnabled())
        {
            return _disabledTimer;
        }

        var ret = new DiagnosticTimer()
        {
            _collectionName = collectionName,
            _methodName = methodName,
            _clusterName = clusterName,
            _stopwatch = Stopwatch.StartNew(),
        };

        return ret;
    }

    public void SetSuccess()
    {
        _isSuccessful = true;
    }

    private void StopAndWriteDiagnostics()
    {
        if (_stopwatch == null)
        {
            // Means we have a disabled timer
            return;
        }

        if (!_stopwatch.IsRunning)
        {
            // Already stopped
            return;
        }

        _stopwatch.Stop();

        if (!QdrantHttpClientDiagnosticSource.Instance.IsEnabled())
        {
            return;
        }

        QdrantHttpClientDiagnosticSource.Instance.Write(
            QdrantHttpClientDiagnosticSource.RequestDurationDiagnosticName,
            new
            {
                collectionName = _collectionName,
                methodName = _methodName,
                duration = _stopwatch.Elapsed.TotalSeconds,
                clusterName = _clusterName,
            }
        );

        QdrantHttpClientDiagnosticSource.Instance.Write(
            QdrantHttpClientDiagnosticSource.RequestsTotalDiagnosticName,
            new
            {
                collectionName = _collectionName,
                methodName = _methodName,
                isSuccessful = _isSuccessful ? "1" : "0",
                clusterName = _clusterName,
            }
        );
    }

    public void Dispose() => StopAndWriteDiagnostics();
}
