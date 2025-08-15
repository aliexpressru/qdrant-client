using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// The request for updating an existing Qdrant collection parameters.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class UpdateCollectionParametersRequest
{
    #region Nested classes

    /// <summary>
    /// Represents basic collection parameters.
    /// </summary>
    public class CollectionParameters
    {
        /// <summary>
        /// Number of shards replicas. Default is 1 Minimum is 1.
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
        /// If <c>true</c> - point's payload will not be stored in memory.
        /// It will be read from the disk every time it is requested.
        /// This setting saves RAM by (slightly) increasing the response time.
        /// Those payload values that are involved in filtering and are indexed - remain in RAM.
        /// </summary>
        public bool? OnDiskPayload { get; set; }
    }

    #endregion
    
    internal bool IsEmpty { private init; get; }

    /// <summary>
    /// Used to issue an empty update collection parameters request.
    /// As per documentation https://qdrant.tech/documentation/concepts/collections/#grey-collection-status
    /// this should be done to trigger <see cref="QdrantCollectionStatus.Grey"/> status collection optimizers. 
    /// </summary>
    internal static string EmptyRequestBody { get; } = """
        {
            "optimizers_config": {}
        }
        """;

    /// <summary>
    /// Represents an empty update collection parameters request.
    /// As per documentation https://qdrant.tech/documentation/concepts/collections/#grey-collection-status such
    /// request should be issued to trigger optimizers for collections in <see cref="QdrantCollectionStatus.Grey"/> status.
    /// </summary>
    public static UpdateCollectionParametersRequest Empty { get; } = new() {IsEmpty = true};

    /// <summary>
    /// The dense vector configuration to update.
    /// </summary>
    [JsonConverter(typeof(VectorConfigurationJsonConverter))]
    public VectorConfigurationBase Vectors { get; set; }

    /// <summary>
    /// Gets or sets the optimizers configuration.
    /// </summary>
    public OptimizersConfiguration OptimizersConfig { get; set; }

    /// <summary>
    /// Gets or sets the basic collection parameters.
    /// </summary>
    public CollectionParameters Params { get; set; }

    /// <summary>
    /// The HNSW configuration to update.
    /// </summary>
    public HnswConfiguration HnswConfig { get; set; }

    /// <summary>
    /// The quantization configuration to update.
    /// </summary>
    public QuantizationConfiguration QuantizationConfig { get; set; }

    /// <summary>
    /// The sparse vector configuration to update.
    /// </summary>
    public Dictionary<string, SparseVectorConfiguration> SparseVectors { get; set; }

    /// <summary>
    /// Strict mode configuration.
    /// </summary>
    public StrictModeConfiguration StrictModeConfig { get; set; }
}
