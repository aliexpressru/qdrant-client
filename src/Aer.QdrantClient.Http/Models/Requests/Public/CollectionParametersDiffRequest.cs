using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// The request for updating an existing Qdrant collection parameters using diff types.
/// All fields are optional — only specified fields will be updated.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class CollectionParametersDiffRequest
{
    /// <summary>
    /// Represents basic collection parameters diff.
    /// </summary>
    public sealed class CollectionParametersDiff
    {
        /// <summary>
        /// Number of shards replicas. Default is 1. Minimum is 1.
        /// </summary>
        public uint? ReplicationFactor { get; set; }

        /// <summary>
        /// Defines how many replicas should apply the operation for us to consider it successful.
        /// Increasing this number will make the collection more resilient to inconsistencies,
        /// but will also make it fail if not enough replicas are available. Does not have any performance impact.
        /// </summary>
        public uint? WriteConsistencyFactor { get; set; }

        /// <summary>
        /// Fan-out every read request to these many additional remote nodes (and return first available response).
        /// </summary>
        public uint? ReadFanOutFactor { get; set; }

        /// <summary>
        /// Define number of milliseconds to wait before attempting to read from another replica.
        /// This setting can help to reduce latency spikes in case of occasional slow replicas.
        /// Default is 0, which means delayed fan out request is disabled.
        /// </summary>
        public ulong? ReadFanOutDelayMs { get; set; }

        /// <summary>
        /// If <c>true</c> - point's payload will not be stored in memory.
        /// It will be read from the disk every time it is requested.
        /// This setting saves RAM by (slightly) increasing the response time.
        /// Those payload values that are involved in filtering and are indexed - remain in RAM.
        /// </summary>
        public bool? OnDiskPayload { get; set; }
    }

    /// <summary>
    /// The dense vector configuration diff to update.
    /// </summary>
    [JsonConverter(typeof(VectorConfigurationDiffJsonConverter))]
    public VectorConfigurationDiff Vectors { get; set; }

    /// <summary>
    /// Gets or sets the optimizers configuration diff.
    /// </summary>
    public OptimizersConfigurationDiff OptimizersConfig { get; set; }

    /// <summary>
    /// Gets or sets the basic collection parameters diff.
    /// </summary>
    public CollectionParametersDiff Params { get; set; }

    /// <summary>
    /// The HNSW configuration diff to update.
    /// </summary>
    public HnswConfigurationDiff HnswConfig { get; set; }

    /// <summary>
    /// The quantization configuration diff to update.
    /// </summary>
    [JsonConverter(typeof(QuantizationConfigurationDiffJsonConverter))]
    public QuantizationConfigurationDiff QuantizationConfig { get; set; }

    /// <summary>
    /// The sparse vector configuration to update.
    /// </summary>
    public Dictionary<string, SparseVectorConfiguration> SparseVectors { get; set; }

    /// <summary>
    /// Strict mode configuration.
    /// </summary>
    public StrictModeConfiguration StrictModeConfig { get; set; }

    /// <summary>
    /// Metadata to update for the collection. If provided, this will merge with existing metadata.
    /// To remove metadata, set it to an empty object.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }
}
