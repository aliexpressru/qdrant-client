// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The optimizers configuration.
/// </summary>
public class OptimizersConfiguration
{
    /// <summary>
    /// The minimal fraction of deleted vectors in a segment, required to perform segment optimization.
    /// </summary>
    public double? DeletedThreshold { set; get; }

    /// <summary>
    /// The minimal number of vectors in a segment, required to perform segment optimization.
    /// </summary>
    public ulong? VacuumMinVectorNumber { set; get; }

    /// <summary>
    /// Target amount of segments optimizer will try to keep. Real amount of segments may vary
    /// depending on multiple parameters: - Amount of stored points - Current write RPS
    /// It is recommended to select default number of segments as a factor of the number
    /// of search threads, so that each segment would be handled evenly by one of the
    /// threads If default_segment_number = 0, will be automatically selected by the number of available CPUs
    /// </summary>
    public ulong? DefaultSegmentNumber { set; get; }

    /// <summary>
    /// Do not create segments larger this size (in KiloBytes). Large segments might require
    /// disproportionately long indexation times, therefore it makes sense to limit the size of segments.
    /// If indexation speed have more priority for your - make this parameter lower.
    /// If search speed is more important - make this parameter higher. 1Kb = 1 vector of size 256
    /// </summary>
    public ulong? MaxSegmentSize { set; get; }

    /// <summary>
    /// Maximum size (in KiloBytes) of vectors to store in-memory per segment.
    /// Segments larger than this threshold will be stored as read-only memmaped file.
    /// To enable memmap storage, lower the threshold. 1Kb = 1 vector of size 256
    /// </summary>
    public ulong? MemmapThreshold { set; get; }

    /// <summary>
    /// Maximum size (in KiloBytes) of vectors allowed for plain index.
    /// To disable vector indexing, set to <c>0</c>.
    /// Note: 1kB = 1 vector of size 256.
    /// </summary>
    public ulong? IndexingThreshold { set; get; }

    /// <summary>
    /// Minimum interval between forced flushes.
    /// </summary>
    public ulong? FlushIntervalSec { set; get; }

    /// <summary>
    /// Maximum available threads for optimization workers.
    /// </summary>
    public ulong? MaxOptimizationThreads { set; get; }
}
