using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Replaces the cluster-wide resource quota configuration.
    /// The new configuration is propagated to every peer through consensus and persisted, so it survives restarts.
    /// </summary>
    /// <param name="enabled">Specifies whether the limits are enforced. Default is false.</param>
    /// <param name="maxResidentMemoryPercent">
    /// Rejects memory-consuming updates once process resident memory reaches this percentage of total system memory (or of the cgroup limit, if one applies).
    /// Values in range [1..100]
    /// </param>
    /// <param name="maxDiskUsagePercent">
    /// Rejects disk-consuming updates once the filesystem hosting the storage directory is filled to this percentage of its capacity.
    /// Values in range [1..100]
    /// </param>
    /// <param name="releaseMarginPercent">
    /// Determines how many percentage points below its limit a resource has to fall before this node starts accepting work again.
    /// Values in range [0..100]
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The cluster name.</param>
    /// <param name="isWaitForResult">If true, wait until the new configuration is confirmed by consensus on this peer. If false - the request returns as soon as the change is proposed.</param>
    Task<SetQuotasResponse> SetQuotas(
        bool enabled,
        uint? maxResidentMemoryPercent,
        uint? maxDiskUsagePercent,
        uint? releaseMarginPercent, 
        CancellationToken cancellationToken,
        string clusterName = null,
        bool isWaitForResult = true);
    
    /// <summary>
    /// Gets the cluster-wide resource quota configuration, together with the current utilization it is measured against. 
    /// </summary>
    /// <remarks>
    /// The configuration is the same on every peer, but the reported utilization is for the node serving this request only.
    /// Memory and disk are node-local, so query each peer to see where the whole cluster stands.
    /// </remarks>>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The cluster name.</param>
    Task<GetQuotasResponse> GetQuotas(
        CancellationToken cancellationToken,
        string clusterName = null);
}