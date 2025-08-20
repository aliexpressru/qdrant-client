using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the detailed collection information.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetCollectionInfoResponse : QdrantResponseBase<GetCollectionInfoResponse.CollectionInfo>
{
    /// <summary>
    /// Represents a single collection information.
    /// </summary>
    public class CollectionInfo
    {
        /// <summary>
        /// Current state of the collection.
        /// Green - all good.
        /// Yellow - optimization is running,
        /// Red - some operations failed and were not recovered.
        /// </summary>
        public QdrantCollectionStatus Status { get; set; }

        /// <summary>
        /// The collection optimizer status.
        /// </summary>
        [JsonConverter(typeof(QdrantCollectionOptimizerStatusJsonConverter))]
        public QdrantOptimizerStatusUint OptimizerStatus { get; set; }

        /// <summary>
        /// Approximate number of indexed vectors in the collection.
        /// </summary>
        public ulong? IndexedVectorsCount { get; set; }

        /// <summary>
        /// Approximate number of points (vectors + payloads) in collection.
        /// </summary>
        public ulong? PointsCount { get; set; }

        /// <summary>
        /// The total collection segments (storage units) count.
        /// </summary>
        public ulong? SegmentsCount { get; set; }

        /// <summary>
        /// The collection configuration.
        /// </summary>
        public CollectionConfiguration Config { get; set; }

        /// <summary>
        /// The indexed payload fields configurations by field names.
        /// </summary>
        public Dictionary<string, PayloadSchemaPropertyDefinition> PayloadSchema { set; get; }
    }

    /// <summary>
    /// Represents qdrant collection optimizer status.
    /// </summary>
    public class QdrantOptimizerStatusUint
    {
        /// <summary>
        /// The qdrant collection optimizer status.
        /// </summary>
        public QdrantOptimizerStatus Status { get; }

        /// <summary>
        /// Determines whether optimizers are reporting as expected.
        /// </summary>
        public bool IsOk => Status == QdrantOptimizerStatus.Ok;

        /// <summary>
        /// The qdrant collection optimizer error message. If no error occurred the value is <c>null</c>.
        /// </summary>
        public string Error { get; init; }

        /// <summary>
        /// Gets the raw optimizer status string. This property has a value only in case of an unknown status.
        /// </summary>
        public string RawStatusString { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QdrantOptimizerStatusUint"/> class.
        /// </summary>
        /// <param name="status">The status type.</param>
        public QdrantOptimizerStatusUint(QdrantOptimizerStatus status)
        {
            Status = status;
        }
    }

    /// <summary>
    /// Represents the collection configuration.
    /// </summary>
    public class CollectionConfiguration
    {
        /// <summary>
        /// The collection parameters.
        /// </summary>
        public Parameters Params { get; set; }

        /// <summary>
        /// The HNSW index configuration.
        /// </summary>
        public HnswConfiguration HnswConfig { get; set; }

        /// <summary>
        /// The optimizer configuration.
        /// </summary>
        public OptimizersConfiguration OptimizerConfig { get; set; }

        /// <summary>
        /// The write-ahead log configuration.
        /// </summary>
        public WalConfiguration WalConfig { get; set; }

        /// <summary>
        /// Quantization configuration.
        /// </summary>
        [JsonConverter(typeof(QuantizationConfigurationJsonConverter))]
        public QuantizationConfiguration QuantizationConfig { get; set; }

        /// <summary>
        /// Strict mode configuration.
        /// </summary>
        public StrictModeConfiguration StrictModeConfig { get; set; }

        /// <summary>
        /// Represents the collection parameters.
        /// </summary>
        public class Parameters
        {
            /// <summary>
            /// The vector parameters.
            /// </summary>
            [JsonConverter(typeof(VectorConfigurationJsonConverter))]
            public VectorConfigurationBase Vectors { set; get; }

            /// <summary>
            /// The shard number.
            /// </summary>
            public uint? ShardNumber { set; get; }

            /// <summary>
            /// The sharding method. This field will have value of <see cref="ShardingMethod.Custom"/> if custom sharding is configured.
            /// </summary>
            /// <remarks>
            /// In this mode, the <see cref="ShardNumber"/> means the number of shards per shard key,
            /// where points will be distributed evenly.
            /// </remarks>
            public ShardingMethod? ShardingMethod { set; get; }

            /// <summary>
            /// The replication factor.
            /// </summary>
            public uint? ReplicationFactor { set; get; }

            /// <summary>
            /// The write consistency factor.
            /// </summary>
            public uint? WriteConsistencyFactor { set; get; }

            /// <summary>
            /// Whether the payload is stored on disk or in memory.
            /// </summary>
            public bool OnDiskPayload { set; get; }

            /// <summary>
            /// Represents sparse vectors configuration.
            /// </summary>
            public Dictionary<string, SparseVectorConfiguration> SparseVectors { get; set; }
        }

        /// <summary>
        /// Represents the write-ahead log configuration.
        /// </summary>
        public class WalConfiguration
        {
            /// <summary>
            /// The write-ahead log capacity in megabytes.
            /// </summary>
            public ulong? WalCapacityMb { set; get; }

            /// <summary>
            /// The number of the WAL segments ahead (?)
            /// </summary>
            public ulong WalSegmentsAhead { set; get; }
        }
    }

    /// <summary>
    /// Represents one payload indexed field schema.
    /// </summary>
    public class PayloadSchemaPropertyDefinition
    {
        /// <summary>
        /// The payload field data type.
        /// </summary>
        public PayloadIndexedFieldType DataType { set; get; }

        /// <summary>
        /// The payload field indexing parameters.
        /// </summary>
        public PayloadSchemaPropertyParameters Params { set; get; }

        /// <summary>
        /// The points count in this schema.
        /// </summary>
        public long Points { set; get; }

        /// <summary>
        /// Represents the payload field indexing parameters.
        /// </summary>
        public class PayloadSchemaPropertyParameters
        {
            /// <summary>
            /// The index type.
            /// </summary>
            public string Type { set; get; }

            /// <summary>
            /// The tokenizer type. For full-text index only.
            /// </summary>
            public FullTextIndexTokenizerType? Tokenizer { set; get; }

            /// <summary>
            /// The minimal token length. For full-text index only.
            /// </summary>
            public ulong? MinTokenLen { set; get; }

            /// <summary>
            /// The maximal token length. For full-text index only.
            /// </summary>
            public ulong? MaxTokenLen { set; get; }

            /// <summary>
            /// If <c>true</c> - full-text index supports phrase matching. Default: <c>false</c>.
            /// </summary>
            public bool? PhraseMatching { set; get; }

            /// <summary>
            /// Ignore this set of tokens. Can select from predefined languages and/or provide a custom set.
            /// </summary>
            [JsonConverter(typeof(FullTextIndexStopwordsJsonConverter))]
            public FullTextIndexStopwords Stopwords { set; get; }

            /// <summary>
            /// Algorithm for stemming. Default: disabled.
            /// </summary>
            [JsonConverter(typeof(FullTextIndexStemmingAlgorithmJsonConverter))]
            public FullTextIndexStemmingAlgorithm Stemmer { set; get; }

            /// <summary>
            /// Whether to convert tokens to lowercase before indexing. For full-text index only.
            /// </summary>
            public bool? Lowercase { set; get; }

            /// <summary>
            /// If <c>true</c> - integer index supports ranges filters. Default is <c>true</c> for integer indexes.
            /// </summary>
            public bool? Range { set; get; }

            /// <summary>
            /// If <c>true</c> - integer index supports direct lookups. Default is <c>true</c> for integer indexes.
            /// </summary>
            public bool? Lookup { set; get; }

            /// <summary>
            /// Whether the payload index is stored on-disk instead of in-memory.
            /// </summary>
            public bool OnDisk { set; get; }

            /// <summary>
            /// Whether the field index is a tenant index.
            /// </summary>
            public bool? IsTenant { set; get; }

            /// <summary>
            /// Whether the field index is a principal index.
            /// </summary>
            public bool? IsPrincipal { set; get; }
        }
    }
}
