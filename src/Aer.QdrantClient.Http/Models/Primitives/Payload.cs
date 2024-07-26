using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Infrastructure.Json;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents the point payload.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class Payload
{
    /// <summary>
    /// Gets the raw JSON string for this payload.
    /// </summary>
    /// <remarks>For get responses only.</remarks>
    public JsonObject RawPayload { get; init; }

    /// <summary>
    /// Parses the payload as object of specified type.
    /// </summary>
    /// <typeparam name="T">The type of the deserialized payload object.</typeparam>
    public T As<T>()
        where T : class
        => RawPayload?.Deserialize<T>(JsonSerializerConstants.SerializerOptions);

    /// <summary>
    /// Returns raw json string representation.
    /// </summary>
    /// <param name="isFormatPayloadJson">Determines whether the resulting json string should be formatted.</param>
    public string ToString(bool isFormatPayloadJson)
        =>
            RawPayload?.ToJsonString(
                isFormatPayloadJson
                    ? JsonSerializerConstants.IndentedSerializerOptions
                    : JsonSerializerConstants.SerializerOptions);

    /// <inheritdoc/>
    public override string ToString() => ToString(false);
}
