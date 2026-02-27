using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a point id or a search vector - dense or sparse or an inference object to create vector form a text, image or other object.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class PointIdOrVectorOrInferenceModel
{
    /// <summary>
    /// The point id.
    /// </summary>
    [JsonConverter(typeof(PointIdJsonConverter))]
    internal PointId PointId { get; }

    /// <summary>
    /// The search vector.
    /// </summary>
    [JsonConverter(typeof(QueryVectorJsonConverter))]
    internal QueryVector QueryVector { get; }

    /// <summary>
    /// The inference object.
    /// </summary>
    [JsonConverter(typeof(InferenceObjectJsonConverter))]
    internal InferenceObjectBase InferenceObject { get; }

    /// <summary>
    /// Private ctor to enforce only implicit conversions.
    /// </summary>
    private PointIdOrVectorOrInferenceModel()
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="PointIdOrVectorOrInferenceModel"/> with point id.
    /// </summary>
    /// <param name="pointId">The point id.</param>
    private PointIdOrVectorOrInferenceModel(PointId pointId)
    {
        PointId = pointId;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PointIdOrVectorOrInferenceModel"/> with search vector.
    /// </summary>
    /// <param name="queryVector">The search vector.</param>
    private PointIdOrVectorOrInferenceModel(QueryVector queryVector)
    {
        QueryVector = queryVector;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PointIdOrVectorOrInferenceModel"/> with inference object.
    /// </summary>
    /// <param name="inferenceObject">The inference object.</param>
    private PointIdOrVectorOrInferenceModel(InferenceObjectBase inferenceObject)
    {
        InferenceObject = inferenceObject;
    }

    /// <summary>
    /// Implicitly converts <see cref="PointId"/> to <see cref="PointIdOrVectorOrInferenceModel"/>.
    /// </summary>
    /// <param name="pointId">The point id to convert.</param>
    public static implicit operator PointIdOrVectorOrInferenceModel(PointId pointId) => new(pointId);

    /// <summary>
    /// Implicitly converts <see cref="QueryVector"/> to <see cref="PointIdOrVectorOrInferenceModel"/>.
    /// </summary>
    /// <param name="queryVector">The query vector to convert.</param>
    public static implicit operator PointIdOrVectorOrInferenceModel(QueryVector queryVector) => new(queryVector);

    /// <summary>
    /// Implicitly converts <see cref="float"/> dense vector components array to <see cref="PointIdOrVectorOrInferenceModel"/>.
    /// </summary>
    /// <param name="vector">The vector components.</param>
    public static implicit operator PointIdOrVectorOrInferenceModel(float[] vector) => new(vector);

    /// <summary>
    /// Implicitly converts the sparse vector components array to <see cref="PointIdOrVectorOrInferenceModel"/>.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator PointIdOrVectorOrInferenceModel((uint[] Indices, float[] Values) sparseVectorComponents) =>
        new(sparseVectorComponents);

    /// <summary>
    /// Implicitly converts the <see cref="VectorBase"/> to <see cref="PointIdOrVectorOrInferenceModel"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static implicit operator PointIdOrVectorOrInferenceModel(VectorBase vector) => new(vector);

    /// <summary>
    /// Implicitly converts the <see cref="InferenceObjectBase"/> to <see cref="PointIdOrVectorOrInferenceModel"/>.
    /// </summary>
    /// <param name="inferenceObject">The inference object to convert.</param>
    public static implicit operator PointIdOrVectorOrInferenceModel(InferenceObjectBase inferenceObject) => new(inferenceObject);
}
