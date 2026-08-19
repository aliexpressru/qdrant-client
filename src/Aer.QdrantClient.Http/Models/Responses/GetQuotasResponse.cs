using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Gets the cluster-wide resource quota configuration.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetQuotasResponse : QdrantResponseBase<GetQuotasResponse.QuotaResult>
{
    /// <summary>
    /// Represents the cluster-wide resource quota configuration, together with the current utilization it is measured against.
    /// </summary>
    public sealed class QuotaResult
    {
        /// <summary>
        /// Returns utilization reported by each peer, keyed by peer ID.
        /// </summary>
        public Dictionary<string, UsageInfoEx> Peers { get; init; }
        
        /// <summary>
        /// Returns cluster-wide limits on node resources.
        /// </summary>
        public QuotaConfiguration Config { get; init; }

        /// <summary>
        /// Returns utilization of the quota-managed resources on this node alone — memory and disk are node-local.
        /// </summary>
        public UsageInfo Usage { get; init; }
    }
    
    /// <summary>
    /// Represents cluster-wide limits on node resources. Limits are only enforced while enabled is true.
    /// </summary>
    public sealed class QuotaConfiguration
    {
        /// <summary>
        /// Specifies whether the limits are enforced.
        /// </summary>
        public bool Enabled { get; init; }

        /// <summary>
        ///Rejects memory-consuming updates once process resident memory reaches this percentage of total system memory (or of the cgroup limit, if one applies).
        /// </summary>
        public uint? MaxResidentMemoryPercent { get; init; }

        /// <summary>
        /// Rejects disk-consuming updates once the filesystem hosting the storage directory is filled to this percentage of its capacity.
        /// </summary>
        public uint? MaxDiskUsagePercent { get; init; }
        
        /// <summary>
        /// Determines how many percentage points below its limit a resource has to fall before this node starts accepting work again.
        /// </summary>
        public uint? ReleaseMarginPercent { get; init; }
    }

    /// <summary>
    /// Represents utilization reported by a peer with the exceeded field.
    /// </summary>
    public sealed class UsageInfoEx : UsageInfo
    {
        /// <summary>
        /// Returns whether this peer is at or over one of the enforced limits, and so is currently refusing updates. Always false while the quota is disabled.
        /// </summary>
        public bool Exceeded { get; init; }
    }
    
    /// <summary>
    /// Represents utilization of the quota-managed resources on the node.
    /// </summary>
    public class UsageInfo
    {
        /// <summary>
        /// Returns resident memory of this node’s process, as a percentage of the memory available to it (cgroup limit if one applies, else total system memory).
        /// </summary>
        public uint? ResidentMemoryPercent { get; init; }

        /// <summary>
        /// Returns used space of this node's storage filesystem, as a percentage of its capacity.
        /// </summary>
        public uint? DiskUsagePercent { get; init; }
    }
}