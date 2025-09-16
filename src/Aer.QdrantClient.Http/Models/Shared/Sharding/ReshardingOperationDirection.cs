namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The resharding operation direction.
/// </summary>
public enum ReshardingOperationDirection
{
    /// <summary>
    /// Scale up, add a new shard.
    /// </summary>
    Up,
    
    /// <summary>
    /// Scale down, remove a shard.
    /// </summary>
    Down
}
