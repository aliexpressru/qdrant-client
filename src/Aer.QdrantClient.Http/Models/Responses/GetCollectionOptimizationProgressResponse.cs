using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the get collection optimization progress operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetCollectionOptimizationProgressResponse : QdrantResponseBase<GetCollectionOptimizationProgressResponse.CollectionOptimizationProgress>
{
    /// <summary>
    /// Represents the current optimization state of the collection.
    /// </summary>
    public sealed class CollectionOptimizationProgress
    {
        /// <summary>
        /// Optimization summary.
        /// </summary>
        public OptimisationSummaryUnit Summary { get; init; }

        /// <summary>
        /// Currently running optimizations.
        /// </summary>
        public OptimisationInfoUnit[] Running { get; init; }

        /// <summary>
        /// An estimated queue of pending optimizations. Requires to be selected with <see cref="OptimizationProgressOptionalInfoFields.Queued"/>.
        /// </summary>
        public QueuedOptimisationInfoUnit[] Queued { get; init; }

        /// <summary>
        /// Completed optimizations. Requires to be selected with <see cref="OptimizationProgressOptionalInfoFields.Completed"/>.
        /// </summary>
        public OptimisationInfoUnit[] Completed { get; init; }

        /// <summary>
        /// Segments that don’t require optimization.
        /// Requires to be selected with <see cref="OptimizationProgressOptionalInfoFields.IdleSegments"/>.
        /// </summary>
        public SegmentInfoUint[] IdleSegments { get; init; }

        /// <summary>
        /// Represents a queued optimization information.
        /// </summary>
        public sealed class QueuedOptimisationInfoUnit
        {
            /// <summary>
            /// Name of the optimizer that scheduled this optimization.
            /// </summary>
            public string Optimizer { get; init; }

            /// <summary>
            /// Segments that will be optimized.
            /// </summary>
            public SegmentInfoUint[] Segments { get; init; }
        }

        /// <summary>
        /// Represents a running / completed optimization information.
        /// </summary>
        public sealed class OptimisationInfoUnit
        {
            /// <summary>
            /// Unique identifier of the optimization process.
            /// After the optimization is complete, a new segment will be created with this uuid.
            /// </summary>
            public string Uuid { get; init; }

            /// <summary>
            /// Name of the optimizer that performed this optimization.
            /// </summary>
            public string Optimizer { get; init; }

            /// <summary>
            /// Represents the current state of the optimizer being tracked.
            /// </summary>
            public TrackedOptimizerStatus Status { get; init; }

            /// <summary>
            /// Segments being optimized.
            /// After the optimization is complete, these segments will be replaced by the new optimized segments.
            /// </summary>
            public SegmentInfoUint[] Segments { get; init; }

            /// <summary>
            /// The optimizer operations progress.
            /// </summary>
            public OptimiserProgressUnit Progress { get; init; }
        }

        /// <summary>
        /// Represents the tracked optimizer status.
        /// </summary>
        public enum TrackedOptimizerStatus
        {
            /// <summary>
            /// Optimizer is running.
            /// </summary>
            Optimizing,

            /// <summary>
            /// Optimizations finished.
            /// </summary>
            Done,

            /// <summary>
            /// Optimizations were cancelled.
            /// </summary>
            Cancelled,

            /// <summary>
            /// Optimizer encountered an error.
            /// </summary>
            Error
        }

        /// <summary>
        /// Represents an optimized segment information.
        /// </summary>
        public sealed class SegmentInfoUint
        {
            /// <summary>
            /// Unique identifier of the segment.
            /// </summary>
            public string Uuid { get; init; }

            /// <summary>
            /// Number of non-deleted points in the segment.
            /// </summary>
            public int PointsCount { get; init; }
        }

        /// <summary>
        /// Represents an optimizer progress information.
        /// </summary>
        public sealed class OptimiserProgressUnit
        {
            /// <summary>
            /// Name of the optimization operation.
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// When the operation started.
            /// </summary>
            public DateTimeOffset? StartedAt { get; init; }

            /// <summary>
            /// When the operation finished.
            /// </summary>
            public DateTimeOffset? FinishedAt { get; init; }

            /// <summary>
            /// For finished operations, how long they took, in seconds.
            /// </summary>
            public double? DurationSec { get; init; }

            /// <summary>
            /// Number of completed units of work, if applicable.
            /// </summary>
            public ulong? Done { get; init; }

            /// <summary>
            /// Total number of units of work, if applicable and known.
            /// </summary>
            public ulong? Total { get; init; }

            /// <summary>
            /// Child operations.
            /// </summary>
            public OptimiserProgressUnit[] Children { get; init; }
        }

        /// <summary>
        /// Represents optimization summary.
        /// </summary>
        public sealed class OptimisationSummaryUnit
        {
            /// <summary>
            /// Number of pending optimizations in the queue.
            /// Each optimization will take one or more unoptimized segments and produce one optimized segment.
            /// </summary>
            public int QueuedOptimizations { get; init; }

            /// <summary>
            /// Number of unoptimized segments in the queue.
            /// </summary>
            public int QueuedSegments { get; init; }

            /// <summary>
            /// Number of points in unoptimized segments in the queue.
            /// </summary>
            public int QueuedPoints { get; init; }

            /// <summary>
            /// Number of segments that don't require optimization.
            /// </summary>
            public int IdleSegments { get; init; }
        }
    }
}
