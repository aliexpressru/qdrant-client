using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a discover points context pair.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class ContextPair
{
    /// <summary>
    /// Positive point example.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
    public PointIdOrQueryVector Positive { get; init; }

    /// <summary>
    /// Negtive point example.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
    public PointIdOrQueryVector Negative { get; init; }
}
