using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the qdrant payload is found to be of invalid type.
/// </summary>
/// <param name="payloadTypeName">Name of the payload type.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantInvalidPayloadTypeException(string payloadTypeName)
	: Exception(
		$"Payload of type {payloadTypeName} is not supported. Use complex type or a {nameof(Dictionary<string, object>)}. "
		+ $"If you want to upsert raw json payload either use System.Text.Json {nameof(JsonObject)} or Newtonsoft.Json {nameof(JObject)}.");
