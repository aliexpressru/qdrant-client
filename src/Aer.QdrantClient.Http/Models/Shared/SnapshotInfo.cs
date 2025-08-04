using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a snapshot information.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SnapshotInfo
{
    /// <summary>
    /// Snapshot name.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    /// Snapshot creation time. Usually <c>null</c> for now.
    /// </summary>
    public string CreationTime { set; get; }

    /// <summary>
    /// Size in bytes.
    /// </summary>
    public long Size { set; get; }

    /// <summary>
    /// Snapshot size in megabytes.
    /// </summary>
    public double SizeMegabytes => Size / 1024.0 / 1024.0;
}
