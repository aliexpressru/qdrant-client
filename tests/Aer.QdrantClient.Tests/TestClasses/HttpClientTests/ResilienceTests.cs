using Aer.QdrantClient.Http;
using Aer.QdrantClient.Tests.Base;
using Polly.CircuitBreaker;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class ResilienceTests : QdrantTestsBase
{
	[OneTimeSetUp]
	public void Setup()
	{ }

	[Test]
	public async Task CircuitBreaker_ShouldOpen()
	{
		// Following is a very dodgy attempt to simulate failure, but it works nonetheless
		var circuitBreakerStateProvider = new CircuitBreakerStateProvider();

		var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>()
		{
			FailureRatio = 0.0001,
			MinimumThroughput = 2,
			SamplingDuration = TimeSpan.FromMilliseconds(500),
			BreakDuration = TimeSpan.FromSeconds(1),
			ShouldHandle =
				args =>
				{
					Exception exception = args.Outcome.Exception;
					return new ValueTask<bool>(exception != null);
				},
			StateProvider = circuitBreakerStateProvider
		};
		
		Initialize(
			circuitBreakerOptions: circuitBreakerOptions,
			clientTimeout: TimeSpan.FromMilliseconds(100)
		);

		var faultyQdrantClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();

		// Check initial state
		circuitBreakerStateProvider.CircuitState.Should().Be(CircuitState.Closed);

		int operationCancelledExceptionCount = 0;
		int circuitBreakerExceptionCount = 0;
		
		for (int i = 1; i < 100; i++)
		{
			try
			{
				// Cancel the request just after it started which should eventually trip the circuit breaker
				// The trick is to start request with non-cancelled cancellation token
				// and cancel it after the request started. If we start with
				// cancelled cancellation token the resilience pipeline won't work
				
				CancellationTokenSource cts = new();
				// While debugging this test it might be beneficial to set this to 10
				cts.CancelAfter(TimeSpan.FromMilliseconds(1));
				
				await faultyQdrantClient.GetClusterInfo(cts.Token);
			}
			// We know that the exceptions will be thrown, ignore them but count cancellations
			// and broken circuit exceptions
			catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
			{ 
				operationCancelledExceptionCount++;
			}
			catch (Exception ex) when (ex is BrokenCircuitException)
			{
				circuitBreakerExceptionCount++;
			}
			catch
			{
				// We know that the exception will be thrown, ignore it
			}
		}

		operationCancelledExceptionCount.Should().BeGreaterThan(0);
		circuitBreakerExceptionCount.Should().BeGreaterThan(0);

		circuitBreakerStateProvider.CircuitState.Should().Be(CircuitState.Open);
		
		// Wait for the circuit breaker to close again
		await Task.Delay(TimeSpan.FromSeconds(1));

		// Issue request again - it should be fine since we have waited for the circuit breaker to close again
		var getInstanceDetailsResponseAct =
			async () => await faultyQdrantClient.GetInstanceDetails(CancellationToken.None);

		await getInstanceDetailsResponseAct.Should().NotThrowAsync();

		circuitBreakerStateProvider.CircuitState.Should().Be(CircuitState.Closed);
	}
}
