namespace Aer.QdrantClient.Http.HttpClientHanders;

internal class ApiKeyHttpClientHandler : DelegatingHandler
{
    private readonly string _apiKey;
    
    public ApiKeyHttpClientHandler(string apiKey, HttpMessageHandler innerHandler): base(innerHandler)
    {
        _apiKey = apiKey;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add(QdrantHttpClient.ApiKeyHeaderName, _apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
