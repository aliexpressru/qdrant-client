// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// The request for updating an existing Qdrant collection paramters.
/// </summary>
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
    }

    #endregion

    /// <summary>
    /// Gets or sets the optimizers configuration.
    /// </summary>
    public OptimizersConfiguration OptimizersConfig { get; set; }

    /// <summary>
    /// Gets or sets the basic collection parameters.
    /// </summary>
    public CollectionParameters Params { get; set; }
}
