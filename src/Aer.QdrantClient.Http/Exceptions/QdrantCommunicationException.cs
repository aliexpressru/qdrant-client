using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if communicating with qdrant back-end failed.
/// </summary>
/// <param name="method">The Qdrant method.</param>
/// <param name="url">The Qdrant api URL.</param>
/// <param name="statusCode">The Qdrant api response status code.</param>
/// <param name="reasonPhrase">The Qdrant fail reason phrase.</param>
/// <param name="errorContent">The Qdrant fail raw response content.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantCommunicationException(
	string method,
	string url,
	HttpStatusCode statusCode,
	string reasonPhrase,
	string errorContent)
	: Exception(
		$"Qdrant backend {method} {url} response status code {statusCode} does not indicate success.\nReason: {reasonPhrase}.\nContent: {errorContent}");
