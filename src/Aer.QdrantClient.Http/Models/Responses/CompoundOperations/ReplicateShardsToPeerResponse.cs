using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of replicating collections to a cluster node.
/// Note that replicate collections operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public sealed class ReplicateShardsToPeerResponse : QdrantResponseBase<bool>
{ }
