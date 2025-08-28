using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents the location used to lookup vectors.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class VectorsLookupLocation
{
    /// <summary>
    /// The name of the collection to lookup vectors in.
    /// </summary>
    public string Collection { set; get; }

    /// <summary>
    /// Optional name of the vector field within the collection.
    /// If not provided, the default vector field will be used.
    /// </summary>
    public string Vector { set; get; }

    /// <summary>
    /// Specify in which shards to look for the points, if not specified - look in all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { set; get; }
}
