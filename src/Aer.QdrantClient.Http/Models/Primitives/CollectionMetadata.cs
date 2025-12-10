using Aer.QdrantClient.Http.Infrastructure.Json;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents collection metadata : key-value pairs associated with a given collection.
/// </summary>
public class CollectionMetadata
{
    // Key: metadata key, Value: metadata value in JsonElement form
    //private readonly Dictionary<string, JsonElement> _metadata;
    private readonly ConcurrentDictionary<string, object> _deserializedMetadataCache = new();

    /// <summary>
    /// Represents the raw metadata dictionary. Internal for serialization purposes.
    /// </summary>
    /// <remarks>Can't be null.</remarks>
    internal Dictionary<string, JsonElement> RawMetadata { get; }

    /// <summary>
    /// Gets the number of metadata entries associated with the collection.
    /// </summary>
    public int Count => RawMetadata.Count;

    /// <summary>
    /// Gets a read-only collection containing the keys of the metadata entries.
    /// </summary>
    /// <remarks>
    /// The returned collection reflects the current set of metadata keys. If no metadata is present,
    /// the collection will be empty.
    /// </remarks>
    public IReadOnlyCollection<string> Keys => RawMetadata.Keys;

    /// <summary>
    /// Represents an empty collection metadata instance with no items.
    /// </summary>
    /// <remarks>
    /// Use this field to represent a collection with no metadata, rather than creating a new empty
    /// instance. This can be useful for default values or when a method requires a non-null collection metadata
    /// object.
    /// </remarks>
    public static CollectionMetadata Empty { get; } = new([]);

    internal CollectionMetadata(Dictionary<string, JsonElement> metadataValues)
    {
        RawMetadata = metadataValues ?? [];
    }

    /// <summary>
    /// Determines whether the metadata collection contains the specified key.
    /// </summary>
    /// <param name="metadataKey">The key to locate in the metadata collection. Cannot be null.</param>
    /// <returns><c>true</c> if the metadata collection contains an entry with the specified key; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string metadataKey)
    {
        if (string.IsNullOrEmpty(metadataKey))
        {
            ThrowHelper.ThrowArgumentNullException(nameof(metadataKey));
        }

        if (RawMetadata is { Count: 0 })
        {
            return false;
        }

        return RawMetadata.ContainsKey(metadataKey);
    }

    /// <summary>
    /// Gets that metadata value by specified key. If no value is found returns the provided default value.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="metadataKey">The metadata key.</param>
    /// <param name="defaultValue">The default metadata value to return if no key found.</param>
    public T GetValueOrDefault<T>(string metadataKey, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(metadataKey))
        {
            ThrowHelper.ThrowArgumentNullException(nameof(metadataKey));
        }

        if (RawMetadata is { Count: 0 })
        {
            return defaultValue;
        }

        if (_deserializedMetadataCache.TryGetValue(metadataKey, out var cachedValue))
        {
            return (T)cachedValue;
        }

        if (RawMetadata.TryGetValue(metadataKey, out var metadataValue))
        {
            try
            {
                var ret = metadataValue.Deserialize<T>(JsonSerializerConstants.DefaultSerializerOptions) ??
                    throw new InvalidOperationException($"Failed to deserialize metadata value for key '{metadataKey}' as value of type {typeof(T)}");

                _deserializedMetadataCache[metadataKey] = ret;

                return ret;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize metadata value for key '{metadataKey}': {ex.Message}", ex);
            }
        }

        return default;
    }

    /// <summary>
    /// Returns a JSON string representation of the collection metadata.
    /// </summary>
    /// <param name="isFormatJson">Whether to format the JSON string with indentation for readability.</param>
    /// <returns>A JSON-formatted string that represents the metadata of the collection.</returns>
    public string ToString(bool isFormatJson) =>
        JsonSerializer.Serialize(
            RawMetadata,
            isFormatJson
                ? JsonSerializerConstants.DefaultIndentedSerializerOptions
                : JsonSerializerConstants.DefaultSerializerOptions
        );

    /// <summary>
    /// Returns a JSON string representation of the collection metadata.
    /// </summary>
    /// <returns>A JSON-formatted string that represents the metadata of the collection.</returns>
    public override string ToString() => ToString(isFormatJson: false);
}
