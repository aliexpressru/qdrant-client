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
    // To reduce memory footprint caused by storing JsonObject when not needed we use lazy parsing
    private JsonObject _parsedPayloadJson;
    
    /// <summary>
    /// Represents the raw JSON string for an empty payload.
    /// </summary>
    public const string EmptyString = "{}";

    /// <summary>
    /// Gets the empty payload instance.
    /// </summary>
    public static Payload Empty { get; } = new(EmptyString);

    /// <summary>
    /// Gets the raw JSON string for this payload.
    /// </summary>
    public string RawPayloadString { get; init; }

    /// <summary>
    /// Gets the raw JSON object for this payload.
    /// </summary>
    /// <remarks>
    /// Not populated until accessed for the first time.
    /// </remarks>
    public JsonObject RawPayload => GetParsedPayloadJson().AsObject();
    
    /// <summary>
    /// If <c>true</c> indicates that the payload is empty.
    /// </summary>
    public bool IsEmpty =>
        RawPayloadString == null
        || RawPayloadString.Equals(EmptyString, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="Payload"/> class.
    /// </summary>
    /// <param name="rawPayloadString">The raw payload to initialize this instance with.</param>
    public Payload(string rawPayloadString)
    {
        RawPayloadString = rawPayloadString;
    }
    
    /// <summary>
    /// Parses the payload and returns it as an object of specified type.
    /// </summary>
    /// <param name="throwIfEmpty">
    /// Is set to <c>true</c> - throws an exception if this payload is empty.
    /// If set to <c>false</c> - returns <c>null</c>.
    ///
    /// Default is <c>true</c>.
    /// </param>
    /// <typeparam name="T">The type of the deserialized payload object.</typeparam>
    public T As<T>(bool throwIfEmpty = true)
        where T : class
    {
        if (IsEmpty)
        {
            return throwIfEmpty
                ? throw new InvalidOperationException($"Payload is empty and can't be deserialized as {typeof(T)}")
                : null;
        }

        var ret = JsonSerializer.Deserialize<T>(
            RawPayloadString,
            JsonSerializerConstants.DefaultSerializerOptions);

        return ret;
    }

    /// <summary>
    /// Returns raw json string representation.
    /// </summary>
    /// <param name="isFormatPayloadJson">Determines whether the resulting json string should be formatted.</param>
    public string ToString(bool isFormatPayloadJson)
        => IsEmpty
            ? EmptyString
            : isFormatPayloadJson
                ? GetParsedPayloadJson().ToJsonString(JsonSerializerConstants.DefaultIndentedSerializerOptions)
                : RawPayloadString;
            
    /// <inheritdoc/>
    public override string ToString() => ToString(false);
    
    private JsonNode GetParsedPayloadJson()
    {
        if (IsEmpty)
        {
            return new JsonObject();
        }

        _parsedPayloadJson ??= JsonNode.Parse(RawPayloadString)!.AsObject();

        return _parsedPayloadJson;
    }
}
