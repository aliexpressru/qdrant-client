// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The HNSW index configuration.
/// </summary>
public class HnswConfiguration
{
    /// <summary>
    /// Number of edges per node in the index graph. Larger the value - more accurate the search, more space required.
    /// </summary>
    public ulong? M { set; get; }

    /// <summary>
    /// Number of edges per node in the payload index graph. Larger the value - more accurate the search, more space required.
    /// </summary>
    public ulong? PayloadM { set; get; }

    /// <summary>
    /// Number of neighbours to consider during the index building. Larger the value - more accurate the search, more time required to build index.
    /// </summary>
    public ulong? EfConstruct { set; get; }

    /// <summary>
    /// Minimal size (in KiloBytes) of vectors for additional payload-based indexing.
    /// If payload chunk is smaller than full_scan_threshold_kb additional indexing won't be used
    /// - in this case full-scan search should be preferred by query planner and additional
    /// indexing is not required. 1Kb = 1 vector of size 256
    /// </summary>
    public ulong? FullScanThreshold { set; get; }

    /// <summary>
    /// Number of parallel threads used for background index building. If 0 - auto selection.
    /// </summary>
    public ulong? MaxIndexingThreads { set; get; }

    /// <summary>
    /// Store HNSW index on disk. If set to false, index will be stored in RAM. Default: false
    /// </summary>
    public bool? OnDisk { set; get; }
}
