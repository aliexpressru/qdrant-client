using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a vector that used as search request. Either a dense or sparse vector.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class QueryVector
{
    #region Nested classes

    /// <summary>
    /// Represents a dense search vector.
    /// </summary>
    internal sealed class DenseQueryVector : QueryVector
    {
        public float[] Vector { get; }

        public DenseQueryVector(float[] vector)
        {
            Vector = vector;
        }
    }

    /// <summary>
    /// Represents a sparse search vector.
    /// </summary>
    internal sealed class SparseQueryVector : QueryVector
    {
        [JsonConverter(typeof(VectorJsonConverter))]
        public SparseVector Vector { get; }

        public SparseQueryVector(SparseVector vector)
        {
            Vector = vector;
        }
    }

    #endregion

    /// <summary>
    /// Private ctor to enforce only implicit conversions.
    /// </summary>
    private QueryVector()
    { }

    #region Operators

    /// <summary>
    /// Implicitly converts an array of <see cref="float"/> to an instance of <see cref="QueryVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator QueryVector(float[] vector)
    {
        if (vector is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(vector));
        }

        return new DenseQueryVector(vector);
    }

    /// <summary>
    /// Implicitly converts sparse vector components to an instance of <see cref="QueryVector"/>.
    /// </summary>
    /// <param name="sparseVectorComponents">The value to convert.</param>
    public static implicit operator QueryVector((uint[] Indices, float[] Values) sparseVectorComponents) 
        => 
            new SparseQueryVector((SparseVector) sparseVectorComponents);

    /// <summary>
    /// Implicitly converts an instance of <see cref="VectorBase"/> to an instance of <see cref="QueryVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator QueryVector(VectorBase vector)
        =>
            vector switch
            {
                null => throw new ArgumentNullException(nameof(vector)),
                DenseVector v => new DenseQueryVector(v.VectorValues),
                SparseVector sv => new SparseQueryVector(sv),
                _ => throw GetException(vector.GetType())
            };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InvalidCastException GetException(Type vectorType)
        =>
            new(
                $"Can't implicitly cast instance of type {vectorType} to {typeof(QueryVector)}. "
                + $"The value should either be either a dense or a sparse vector");

    #endregion
}
