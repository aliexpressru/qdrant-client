using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// The request for adding a new named vector to an existing Qdrant collection.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class AddNamedVectorRequest
{
    /// <summary>
    /// The dense vector configuration.
    /// </summary>
    public NewDesnseVectorConfiguration Dense { get; }

    /// <summary>
    /// The sparse vector configuration.
    /// </summary>
    public NewSparseVectorConfiguration Sparse { get; }

    public sealed class NewSparseVectorConfiguration
    {
        /// <summary>
        /// Configures additional value modifications for sparse vectors.
        /// </summary>
        public SparseVectorModifier Modifier { set; get; } = SparseVectorModifier.None;

        /// <summary>
        /// Defines which datatype should be used for the index.
        /// </summary>
        [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<VectorDataType>))]
        public VectorDataType Datatype { set; get; } = VectorDataType.Float32;
    }

    public sealed class NewDesnseVectorConfiguration
    {
        /// <summary>
        /// The vector elements count - vector dimensions.
        /// </summary>
        public required ulong Size { get; init; }

        /// <summary>
        /// The distance metric used to build collection index.
        /// </summary>
        public required string Distance { get; init; }

        /// <summary>
        /// The multivector configuration.
        /// </summary>
        public MultivectorConfiguration MultivectorConfig { get; init; }

        /// <summary>
        /// Defines which datatype should be used to represent vectors in the storage.
        /// </summary>
        [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<VectorDataType>))]
        public VectorDataType Datatype { get; init; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AddNamedVectorRequest"/> for adding a dense vector.
    /// </summary>
    /// <param name="denseVectorConfiguration">
    /// The new dense vector configuration.
    /// Only includes properties that define the vector space and cannot be changed after creation.
    /// Storage type, index type, and quantization are inferred.
    /// </param>
    public AddNamedVectorRequest(NewDesnseVectorConfiguration denseVectorConfiguration)
    {
        Dense = denseVectorConfiguration ?? throw new ArgumentNullException(nameof(denseVectorConfiguration));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="AddNamedVectorRequest"/> for adding a sparse vector.
    /// </summary>
    /// <param name="vectorConfiguration">
    /// The new sparse vector configuration. Only includes properties that define the vector space and cannot be changed after creation.
    /// </param>
    public AddNamedVectorRequest(NewSparseVectorConfiguration vectorConfiguration)
    {
        Sparse = vectorConfiguration ?? throw new ArgumentNullException(nameof(vectorConfiguration));
    }
}
