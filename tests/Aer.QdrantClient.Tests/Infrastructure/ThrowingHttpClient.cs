using System.Net;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class ThrowingHttpClient : HttpClient
{
	private readonly HttpClient _client;

	private int _throwCountLeft;
	private int _badRequestCountLeft;

	public ThrowingHttpClient(HttpClient client)
	{
		_client = client;
	}

	public void ThrowOnce()
	{ 
		Interlocked.Increment(ref _throwCountLeft);
	}

	public void BadRequestOnce()
	{
		Interlocked.Increment(ref _badRequestCountLeft);
	}
	
	public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if(_throwCountLeft > 0)
		{
			Interlocked.Decrement(ref _throwCountLeft);
			
			throw new HttpRequestException("ThrowingHttpClient: throwing exception");
		}

		if (_badRequestCountLeft > 0)
		{ 
			Interlocked.Decrement(ref _badRequestCountLeft);
			
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
			{
				Content = new StringContent("ThrowingHttpClient: returning bad request"),
				RequestMessage = request
			};
			
			return Task.FromResult(response);
		}

		return _client.SendAsync(request, cancellationToken);
	}
}
