using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents the request to replace the cluster-wide resource quota configuration.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class SetQuotasRequest
{
    /// <summary>
    /// Specifies whether the limits are enforced.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Rejects memory-consuming updates once process resident memory reaches this percentage of total system memory (or of the cgroup limit, if one applies).
    /// Values in range [1..100]
    /// </summary>
    public uint? MaxResidentMemoryPercent { get; set; }

    /// <summary>
    /// Rejects disk-consuming updates once the filesystem hosting the storage directory is filled to this percentage of its capacity.
    /// Values in range [1..100]
    /// </summary>
    public uint? MaxDiskUsagePercent { get; set; }

    /// <summary>
    /// Determines how many percentage points below its limit a resource has to fall before this node starts accepting work again.
    /// Values in range [0..100]
    /// </summary>
    public uint? ReleaseMarginPercent { get; set; }

    /// <summary>
    /// If true, wait until the new configuration is confirmed by consensus on this peer. If false - the request returns as soon as the change is proposed.
    /// </summary>
    public bool IsWaitForResult { get; set; }
}
