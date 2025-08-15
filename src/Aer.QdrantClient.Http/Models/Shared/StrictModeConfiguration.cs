using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the qdrant strict mode collection configuration.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class StrictModeConfiguration
{
    /// <summary>
    /// Multivector configuration.
    /// </summary>
    public class StrictModeMultivectorConfiguration
    { 
        /// <summary>
        /// Max number of vectors in a multivector.
        /// </summary>
        public uint? MaxVectors { get; set; }
    }

    /// <summary>
    /// Sparse vector configuration.
    /// </summary>
    public class StrictModeSparseVectorConfiguration
    {
        /// <summary>
        /// Max length of sparse vector.
        /// </summary>
        public uint? MaxLength { get; set; }
    }
    
    /// <summary>
    /// Whether strict mode is enabled for a collection or not.
    /// </summary>
    public bool? Enabled { get; set; }
    
    /// <summary>
    /// Max allowed <c>limit</c> parameter for all APIs that don’t have their own max limit.
    /// </summary>
    public uint? MaxQueryLimit { get; set; }
    
    /// <summary>
    /// Max allowed <c>timeout</c> parameter.
    /// </summary>
    public uint? MaxTimeout { get; set; }
    
    /// <summary>
    /// Allow usage of unindexed fields in retrieval based (e.g. search) filters.
    /// </summary>
    public bool? UnindexedFilteringRetrieve { get; set; }
    
    /// <summary>
    /// Allow usage of unindexed fields in filtered updates (e.g. delete by payload).
    /// </summary>
    public bool? UnindexedFilteringUpdate { get; set; }
    
    /// <summary>
    /// Max HNSW value allowed in search parameters.
    /// </summary>
    public uint? SearchMaxHnswEf { get; set; }

    /// <summary>
    /// Whether exact search is allowed or not.
    /// </summary>
    public bool? SearchAllowExact { get; set; }

    /// <summary>
    /// Max oversampling value allowed in search.
    /// </summary>
    public double? SearchMaxOversampling { get; set; }

    /// <summary>
    /// Max batchsize when upserting.
    /// </summary>
    public uint? UpsertMaxBatchsize { get; set; }

    /// <summary>
    /// Max size of a collections vector storage in bytes, ignoring replicas.
    /// </summary>
    public uint? MaxCollectionVectorSizeBytes { get; set; }
    
    /// <summary>
    /// Max number of read operations per minute per replica.
    /// </summary>
    public uint? ReadRateLimit { get; set; }
    
    /// <summary>
    /// Max number of write operations per minute per replica.
    /// </summary>
    public uint? WriteRateLimit { get; set; }
    
    /// <summary>
    /// Max size of a collections payload storage in bytes.
    /// </summary>
    public uint? MaxCollectionPayloadSizeBytes { get; set; }
    
    /// <summary>
    /// Max number of points estimated in a collection.
    /// </summary>
    public uint? MaxPointsCount { get; set; }
    
    /// <summary>
    /// Max conditions a filter can have.
    /// </summary>
    public uint? FilterMaxConditions { get; set; }
    
    /// <summary>
    /// Max size of a condition, e.g. items in <c>MatchAny</c>.
    /// </summary>
    public uint? ConditionMaxSize { get; set; }

    /// <summary>
    /// Multivector configuration.
    /// </summary>
    public Dictionary<string, StrictModeMultivectorConfiguration> MultivectorConfig { get; set; }
    
    /// <summary>
    /// Sparse vector configuration.
    /// </summary>
    public Dictionary<string, StrictModeSparseVectorConfiguration> SparseConfig { get; set; }
}
