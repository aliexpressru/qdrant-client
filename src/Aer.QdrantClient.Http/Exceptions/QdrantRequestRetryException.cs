using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Special type of exception that is used to indicate that the request is being retried.
/// </summary>
/// <param name="responseThatCausedRetry">The response that caused the retry to be executed.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantRequestRetryException(HttpResponseMessage responseThatCausedRetry)
	: Exception(
		// There are loads of conditional access operators in the following string interpolation, due to possibility of RequestMessage being null.
		// This is an exotic case though. See https://github.com/dotnet/runtime/discussions/104113 for details.
		$"{responseThatCausedRetry.RequestMessage?.Method ?? new HttpMethod("UNKNOWN")} request to {responseThatCausedRetry.RequestMessage?.RequestUri ?? new Uri("/unknown", UriKind.Relative)} will be retried because of the result status code {responseThatCausedRetry.StatusCode}.");
