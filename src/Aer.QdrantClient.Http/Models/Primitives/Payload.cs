using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Infrastructure.Json;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents the point payload.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class Payload
{
    // To reduce memory footprint caused by storing JsonObject when it is not needed we don't populate this right away
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
    public string RawPayloadString { get; }

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
        RawPayloadString = string.IsNullOrEmpty(rawPayloadString)
            ? EmptyString
            : rawPayloadString;
    }

    /// <summary>
    /// Gets the specified field as a <see cref="JsonNode"/> from parsed payload json object.
    /// </summary>
    /// <param name="fieldName">The name of the field to get.</param>
    /// <exception cref="NotSupportedException">
    /// Occurs when trying to get a nested field e.g. <c>some.field</c>. Which is not supported yet
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Occurs when specified field is not found in payload json.
    /// </exception>
    public JsonNode this[string fieldName]
    {
        get {
            if (fieldName.Contains('.'))
            {
                // Means we are trying to access a nested property. This is not supported yet

                throw new NotSupportedException($"Getting nested payload property is not supported. Requested property '{fieldName}'");
            }

            if (!RawPayload.ContainsKey(fieldName))
            {
                throw new KeyNotFoundException($"Payload property not found: {fieldName}");
            }

            var payloadProperty = RawPayload[fieldName];

            return payloadProperty;
        }
    }

    /// <summary>
    /// Determines whether the payload contains the specified field.
    /// </summary>
    /// <param name="fieldName">The field to check.</param>
    public bool ContainsField(string fieldName)
    {
        if (fieldName.Contains('.'))
        {
            // Means we are trying to access a nested property. This is not supported yet

            return false;
        }

        return RawPayload.ContainsKey(fieldName);
    }

    /// <summary>
    /// Tries to get the value of the specified payload field.
    /// If the field is not found or can't be converted to specified type - returns <c>false</c>.
    /// </summary>
    /// <param name="fieldName">The name of the field to get value for.</param>
    /// <param name="value">
    /// The obtained typed value of the field if it was found and successfully converted to <typeparamref name="T"/>.
    /// <paramref name="defaultValue"/> otherwise.
    /// </param>
    /// <param name="defaultValue">The default value to return if field is not found.</param>
    /// <typeparam name="T">The type of the value to get.</typeparam>
    public bool TryGetValue<T>(string fieldName, out T value, T defaultValue = default)
    {
        value = defaultValue;

        if (fieldName.Contains('.'))
        {
            // Means we are trying to access a nested property. This is not supported yet

            return false;
        }

        if (!RawPayload.ContainsKey(fieldName))
        {
            return false;
        }

        var payloadField = RawPayload[fieldName];

        if (payloadField is null)
        {
            return false;
        }

        try
        {
            value = payloadField.GetValue<T>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the value of the specified payload field.
    /// </summary>
    /// <param name="fieldName">The name of the field to get value for.</param>
    /// <typeparam name="T">The type of the value to get.</typeparam>
    /// <exception cref="KeyNotFoundException">
    /// Occurs when specified field is not found in payload json.
    /// </exception>
    public T GetValue<T>(string fieldName) => this[fieldName].GetValue<T>();

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
    [OverloadResolutionPriority(1)]
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

    private JsonObject GetParsedPayloadJson()
    {
        if (IsEmpty)
        {
#pragma warning disable IDE0028 // Simplify collection initialization | Justification: clearer this way
            return new();
#pragma warning restore IDE0028 // Simplify collection initialization
        }

        _parsedPayloadJson ??= JsonNode.Parse(RawPayloadString)!.AsObject();

        return _parsedPayloadJson;
    }
}
