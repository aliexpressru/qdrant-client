namespace Aer.QdrantClient.Http.HttpClientHanders;

internal class ApiKeyHttpClientHandler(string apiKey, HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add(QdrantHttpClient.ApiKeyHeaderName, apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
