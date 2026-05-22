using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the get collection memory report response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class CollectionMemoryReportResponse : QdrantResponseBase<CollectionMemoryReportResponse.CollectionMemoryReport>
{
    /// <summary>
    /// Represents the collection memory report.
    /// </summary>
    public sealed class CollectionMemoryReport
    {
        /// <summary>
        /// Total memory usage across all collection components.
        /// </summary>
        public MemoryUsage Total { get; init; }

        /// <summary>
        /// Per-dense-vector memory usage including storage and index.
        /// </summary>
        public VectorMemoryReport[] Vectors { get; init; }

        /// <summary>
        /// Per-sparse-vector memory usage including storage and index.
        /// </summary>
        public VectorMemoryReport[] SparseVectors { get; init; }

        /// <summary>
        /// Payload storage memory usage.
        /// </summary>
        public MemoryUsage Payload { get; init; }

        /// <summary>
        /// Per-payload-field index memory usage.
        /// </summary>
        public PayloadIndexMemoryReport[] PayloadIndex { get; init; }

        /// <summary>
        /// Memory usage for other internal collection components.
        /// </summary>
        public OtherComponentsMemoryReport Other { get; init; }
    }

    /// <summary>
    /// Represents memory usage metrics for a single storage component.
    /// </summary>
    public sealed class MemoryUsage
    {
        /// <summary>
        /// Total bytes stored on disk (file sizes).
        /// </summary>
        public ulong DiskBytes { get; init; }

        /// <summary>
        /// Non-evictable heap RAM: in-memory data structures not backed by mmap.
        /// </summary>
        public ulong RamBytes { get; init; }

        /// <summary>
        /// Evictable RAM: resident file pages from mmap (OS page cache).
        /// </summary>
        public ulong CachedBytes { get; init; }

        /// <summary>
        /// Bytes that should ideally be cached for best performance.
        /// Sum of file sizes for <c>Cached</c> intent files (mmap-accessed data).
        /// </summary>
        public ulong ExpectedCacheBytes { get; init; }
    }

    /// <summary>
    /// Represents memory usage for a named vector, split into storage and index components.
    /// </summary>
    public sealed class VectorMemoryReport
    {
        /// <summary>
        /// Name of the vector.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Memory usage for the raw vector data storage.
        /// </summary>
        public MemoryUsage Storage { get; init; }

        /// <summary>
        /// Memory usage for the vector index (e.g., HNSW graph, quantization data).
        /// </summary>
        public MemoryUsage Index { get; init; }
    }

    /// <summary>
    /// Represents memory usage for a named payload field index.
    /// </summary>
    public sealed class PayloadIndexMemoryReport
    {
        /// <summary>
        /// Name of the indexed payload field.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Memory usage for the payload field index.
        /// </summary>
        public MemoryUsage Usage { get; init; }
    }

    /// <summary>
    /// Represents memory usage for other internal collection components.
    /// </summary>
    public sealed class OtherComponentsMemoryReport
    {
        /// <summary>
        /// Memory usage for the internal point ID tracker.
        /// </summary>
        public MemoryUsage IdTracker { get; init; }
    }
}
