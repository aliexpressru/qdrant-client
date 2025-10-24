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
    /// Gets the empty payload instance.
    /// </summary>
    public static Payload Empty { get; } = new()
    {
        RawPayload = new JsonObject()
    };

    /// <summary>
    /// If to <c>true</c> indicates that the payload is empty.
    /// </summary>
    public bool IsEmpty => RawPayload == null || RawPayload.Count == 0;

    /// <summary>
    /// Parses the payload as object of specified type.
    /// </summary>
    /// <typeparam name="T">The type of the deserialized payload object.</typeparam>
    public T As<T>()
        where T : class
    {
        if (IsEmpty)
        { 
            throw new InvalidOperationException($"Payload is empty and can't be deserialized as {typeof(T)}");
        }

        var ret = RawPayload?.Deserialize<T>(JsonSerializerConstants.DefaultSerializerOptions);

        return ret;
    }

    /// <summary>
    /// Returns raw json string representation.
    /// </summary>
    /// <param name="isFormatPayloadJson">Determines whether the resulting json string should be formatted.</param>
    public string ToString(bool isFormatPayloadJson)
        => IsEmpty
            ? "{}"
            : RawPayload?.ToJsonString(
                isFormatPayloadJson
                    ? JsonSerializerConstants.DefaultIndentedSerializerOptions
                    : JsonSerializerConstants.DefaultSerializerOptions);

    /// <inheritdoc/>
    public override string ToString() => ToString(false);
}
