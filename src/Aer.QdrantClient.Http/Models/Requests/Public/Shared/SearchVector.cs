using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a vector that used as search request. Either an array of <see cref="float"/> without a name
/// or a named vector - array of <see cref="float"/> with associated name property.
/// </summary>
public abstract class SearchVector
{
    #region Nested classes

    /// <summary>
    /// Represents a dense search vector without a name. The single or default vector will be used to perform points search.
    /// </summary>
    internal sealed class DenseSearchVector(float[] vector) : SearchVector
    {
        public float[] Vector { get; } = vector;
    }

    /// <summary>
    /// Represents a search vector without a name. The single or default vector will be used to perform points search.
    /// </summary>
    internal sealed class SparseSearchVector(SparseVector vector) : SearchVector
    {
        [JsonConverter(typeof(VectorJsonConverter))]
        public SparseVector Vector { get; } = vector;
    }

    /// <summary>
    /// Represents a named search vector. The vector with same name will be used to perform points search.
    /// </summary>
    internal sealed class NamedDenseSearchVector(string name, float[] vector) : SearchVector
    {
        public string Name { get; } = name;

        public float[] Vector { get; } = vector;
    }

    /// <summary>
    /// Represents a named search vector. The vector with same name will be used to perform points search.
    /// </summary>
    internal sealed class NamedSparseSearchVector(string name, SparseVector vector) : SearchVector
    {
        public string Name { get; } = name;

        [JsonConverter(typeof(VectorJsonConverter))]
        public VectorBase Vector { get; } = vector;
    }

    #endregion

    /// <summary>
    /// Private ctor to enforce factory methods usage.
    /// </summary>
    private SearchVector()
    { }

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for an unnamed dense vector.
    /// </summary>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(float[] vector) => new DenseSearchVector(vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for a named dense vector.
    /// </summary>
    /// <param name="vectorName">The name of the vector to use in search.</param>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(string vectorName, float[] vector) => new NamedDenseSearchVector(vectorName, vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for an unnamed sparse vector.
    /// </summary>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(SparseVector vector) => new SparseSearchVector(vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for a named sparse vector.
    /// </summary>
    /// <param name="vectorName">The name of the vector to use in search.</param>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(string vectorName, SparseVector vector) => new NamedSparseSearchVector(vectorName, vector);

    #region Operators

    /// <summary>
    /// Implicitly converts an array of <see cref="float"/> to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator SearchVector(float[] vector)
    {
        if (vector is null or { Length: 0 })
        {
            throw new ArgumentNullException(nameof(vector));
        }

        return new DenseSearchVector(vector);
    }

    /// <summary>
    /// Implicitly converts sparse vector components to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="sparseVectorComponents">The value to convert.</param>
    public static implicit operator SearchVector((uint[] Indices, float[] Values) sparseVectorComponents) =>
        new SparseSearchVector((SparseVector)sparseVectorComponents);

    /// <summary>
    /// Implicitly converts an instance of <see cref="VectorBase"/> to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator SearchVector(VectorBase vector)
    {
        switch (vector)
        {
            case null:
                throw new ArgumentNullException(nameof(vector));
            case DenseVector v:
                return new DenseSearchVector(v.VectorValues);
            case NamedVectors { Vectors.Count: 1 } nv:
            {
                var firstVector = nv.Vectors.Single();

                return firstVector.Value.VectorKind switch
                {
                    VectorKind.Dense => new NamedDenseSearchVector(firstVector.Key, firstVector.Value.AsDenseVector().VectorValues),
                    VectorKind.Sparse => new NamedSparseSearchVector(firstVector.Key, (SparseVector)firstVector.Value),
                    _ => throw GetException(vector.GetType())
                };
            }
            case SparseVector sv:
                return new SparseSearchVector(sv);
            default:
                throw GetException(vector.GetType());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InvalidCastException GetException(Type vectorType) =>
        new(
            $"Can't implicitly cast instance of type {vectorType} to {typeof(SearchVector)}. "
            + $"The value should either be an unnamed dense or sparse vector or a named vectors collection with length of 1");

    #endregion
}
