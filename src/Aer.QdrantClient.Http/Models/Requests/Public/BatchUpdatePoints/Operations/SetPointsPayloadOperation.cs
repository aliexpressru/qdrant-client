using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the set points payload operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal sealed class SetPointsPayloadOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Set points payload request.
    /// </summary>
    /// <remarks>
    /// We don't use generic request here due to System.Text.Json limitations.
    /// But this class is internal and never created by end user so we are relatively safe.
    /// </remarks>
    public required object SetPayload { set; get; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }
}
