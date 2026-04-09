using System.Diagnostics;

namespace Aer.QdrantClient.Http.Diagnostics.Helpers;

internal class DiagnosticTimer
{
    private Stopwatch _stopwatch;
    private HttpRequestMessage _request;

    public static DiagnosticTimer StartNew(HttpRequestMessage request) =>
        new() { _request = request, _stopwatch = Stopwatch.StartNew() };

    public void StopAndWriteDiagnostics(bool isSuccessful)
    {
        if (_stopwatch == null)
        {
            return;
        }

        _stopwatch.Stop();

        var endpoint = _request.RequestUri.ToString();

        if (!QdrantHttpClientDiagnosticSource.Instance.IsEnabled())
        {
            return;
        }

        QdrantHttpClientDiagnosticSource.Instance.Write(
            QdrantHttpClientDiagnosticSource.RequestDurationDiagnosticName,
            new { endpoint, duration = _stopwatch.Elapsed.TotalSeconds }
        );

        QdrantHttpClientDiagnosticSource.Instance.Write(
            QdrantHttpClientDiagnosticSource.RequestsTotalDiagnosticName,
            new { endpoint, isSuccessful = isSuccessful ? "1" : "0" }
        );
    }
}
