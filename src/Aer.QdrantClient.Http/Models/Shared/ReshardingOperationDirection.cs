using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The resharding operation direction.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Enum values are used in API and should match the API naming.")]
public enum ReshardingOperationDirection
{
    /// <summary>
    /// Scale up, add a new shard.
    /// </summary>
    up,
    
    /// <summary>
    /// Scale down, remove a shard.
    /// </summary>
    down
}
