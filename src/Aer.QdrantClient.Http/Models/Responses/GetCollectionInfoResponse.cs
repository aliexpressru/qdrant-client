using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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
    public sealed class CollectionInfo
    {
        /// <summary>
        /// Current state of the collection.
        /// Green - all good.
        /// Yellow - optimization is running,
        /// Red - some operations failed and were not recovered.
        /// </summary>
        public QdrantCollectionStatus Status { get; init; }

        /// <summary>
        /// The collection optimizer status.
        /// </summary>
        [JsonConverter(typeof(QdrantCollectionOptimizerStatusJsonConverter))]
        public QdrantOptimizerStatusUint OptimizerStatus { get; init; }

        /// <summary>
        /// Approximate number of indexed vectors in the collection.
        /// </summary>
        public ulong? IndexedVectorsCount { get; init; }

        /// <summary>
        /// Approximate number of points (vectors + payloads) in collection.
        /// </summary>
        public ulong? PointsCount { get; set; }

        /// <summary>
        /// The total collection segments (storage units) count.
        /// </summary>
        public ulong? SegmentsCount { get; init; }

        /// <summary>
        /// The collection configuration.
        /// </summary>
        public CollectionConfiguration Config { get; init; }

        /// <summary>
        /// The indexed payload fields configurations by field names.
        /// </summary>
        public Dictionary<string, PayloadSchemaPropertyDefinition> PayloadSchema { init; get; }

        /// <summary>
        /// Gets the collection metadata if it is present or <c>CollectionMetadata.Empty</c> if it is not.
        /// </summary>
        public CollectionMetadata GetMetadata() => Config?.Metadata ?? CollectionMetadata.Empty;

        /// <summary>
        /// Gets that metadata value by specified key. If no value is found returns the provided default value.
        /// </summary>
        /// <typeparam name="T">The type of the metadata value.</typeparam>
        /// <param name="metadataKey">The metadata key.</param>
        /// <param name="defaultValue">The default metadata value to return if no key found.</param>
        public T GetMetadataValueOrDefault<T>(string metadataKey, T defaultValue = default) =>
            GetMetadata().GetValueOrDefault(metadataKey, defaultValue);

        /// <summary>
        /// Determines whether the metadata collection contains the specified key.
        /// </summary>
        /// <param name="metadataKey">The key to locate in the metadata collection. Cannot be null.</param>
        /// <returns><c>true</c> if the metadata collection contains an entry with the specified key; otherwise, <c>false</c>.</returns>
        public bool ContainsMetadataKey(string metadataKey) => GetMetadata().ContainsKey(metadataKey);
    }

    /// <summary>
    /// Represents qdrant collection optimizer status.
    /// </summary>
    public sealed class QdrantOptimizerStatusUint
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
    public sealed class CollectionConfiguration
    {
        /// <summary>
        /// The collection parameters.
        /// </summary>
        public Parameters Params { get; init; }

        /// <summary>
        /// The HNSW index configuration.
        /// </summary>
        public HnswConfiguration HnswConfig { get; init; }

        /// <summary>
        /// The optimizer configuration.
        /// </summary>
        public OptimizersConfiguration OptimizerConfig { get; init; }

        /// <summary>
        /// The write-ahead log configuration.
        /// </summary>
        public WalConfiguration WalConfig { get; init; }

        /// <summary>
        /// Quantization configuration.
        /// </summary>
        [JsonConverter(typeof(QuantizationConfigurationJsonConverter))]
        public QuantizationConfiguration QuantizationConfig { get; init; }

        /// <summary>
        /// Strict mode configuration.
        /// </summary>
        public StrictModeConfiguration StrictModeConfig { get; init; }

        /// <summary>
        /// Collection metadata.
        /// </summary>
        [JsonConverter(typeof(CollectionMetadataJsonConverter))]
        public CollectionMetadata Metadata { get; init; } = CollectionMetadata.Empty;

        /// <summary>
        /// Represents the collection parameters.
        /// </summary>
        public sealed class Parameters
        {
            /// <summary>
            /// The vector parameters.
            /// </summary>
            [JsonConverter(typeof(VectorConfigurationJsonConverter))]
            public VectorConfigurationBase Vectors { init; get; }

            /// <summary>
            /// The shard number.
            /// </summary>
            public uint? ShardNumber { init; get; }

            /// <summary>
            /// The sharding method. This field will have value of <see cref="ShardingMethod.Custom"/> if custom sharding is configured.
            /// </summary>
            /// <remarks>
            /// In this mode, the <see cref="ShardNumber"/> means the number of shards per shard key,
            /// where points will be distributed evenly.
            /// </remarks>
            public ShardingMethod? ShardingMethod { init; get; }

            /// <summary>
            /// The replication factor.
            /// </summary>
            public uint? ReplicationFactor { init; get; }

            /// <summary>
            /// The write consistency factor.
            /// </summary>
            public uint? WriteConsistencyFactor { init; get; }

            /// <summary>
            /// Whether the payload is stored on disk or in memory.
            /// </summary>
            public bool OnDiskPayload { init; get; }

            /// <summary>
            /// Represents sparse vectors configuration.
            /// </summary>
            public Dictionary<string, SparseVectorConfiguration> SparseVectors { get; init; }
        }

        /// <summary>
        /// Represents the write-ahead log configuration.
        /// </summary>
        public sealed class WalConfiguration
        {
            /// <summary>
            /// The write-ahead log capacity in megabytes.
            /// </summary>
            public ulong? WalCapacityMb { init; get; }

            /// <summary>
            /// The number of the WAL segments ahead (?)
            /// </summary>
            public ulong WalSegmentsAhead { init; get; }
        }
    }

    /// <summary>
    /// Represents one payload indexed field schema.
    /// </summary>
    public sealed class PayloadSchemaPropertyDefinition
    {
        /// <summary>
        /// The payload field data type.
        /// </summary>
        public PayloadIndexedFieldType DataType { init; get; }

        /// <summary>
        /// The payload field indexing parameters.
        /// </summary>
        public PayloadSchemaPropertyParameters Params { init; get; }

        /// <summary>
        /// The points count in this schema.
        /// </summary>
        public long Points { init; get; }

        /// <summary>
        /// Represents the payload field indexing parameters.
        /// </summary>
        public sealed class PayloadSchemaPropertyParameters
        {
            /// <summary>
            /// The index type.
            /// </summary>
            public string Type { init; get; }

            /// <summary>
            /// The tokenizer type. For full-text index only.
            /// </summary>
            public FullTextIndexTokenizerType? Tokenizer { init; get; }

            /// <summary>
            /// The minimal token length. For full-text index only.
            /// </summary>
            public ulong? MinTokenLen { init; get; }

            /// <summary>
            /// The maximal token length. For full-text index only.
            /// </summary>
            public ulong? MaxTokenLen { init; get; }

            /// <summary>
            /// If <c>true</c> - full-text index supports phrase matching. Default: <c>false</c>.
            /// </summary>
            public bool? PhraseMatching { init; get; }

            /// <summary>
            /// Ignore this set of tokens. Can select from predefined languages and/or provide a custom set.
            /// </summary>
            [JsonConverter(typeof(FullTextIndexStopwordsJsonConverter))]
            public FullTextIndexStopwords Stopwords { init; get; }

            /// <summary>
            /// Algorithm for stemming. Default: disabled.
            /// </summary>
            [JsonConverter(typeof(FullTextIndexStemmingAlgorithmJsonConverter))]
            public FullTextIndexStemmingAlgorithm Stemmer { init; get; }

            /// <summary>
            /// Gets the value indicating whether tokens are normalized by folding accented characters to ASCII.
            /// </summary>
            public bool? AsciiFolding { init; get; }

            /// <summary>
            /// Whether to convert tokens to lowercase before indexing. For full-text index only.
            /// </summary>
            public bool? Lowercase { init; get; }

            /// <summary>
            /// If <c>true</c> - integer index supports ranges filters. Default is <c>true</c> for integer indexes.
            /// </summary>
            public bool? Range { init; get; }

            /// <summary>
            /// If <c>true</c> - integer index supports direct lookups. Default is <c>true</c> for integer indexes.
            /// </summary>
            public bool? Lookup { init; get; }

            /// <summary>
            /// Whether the payload index is stored on-disk instead of in-memory.
            /// </summary>
            public bool OnDisk { init; get; }

            /// <summary>
            /// Whether the field index is a tenant index.
            /// </summary>
            public bool? IsTenant { init; get; }

            /// <summary>
            /// Whether the field index is a principal index.
            /// </summary>
            public bool? IsPrincipal { init; get; }
        }
    }
}
