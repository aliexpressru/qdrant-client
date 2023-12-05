// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a vector that used as search request. Either an array of <see cref="float"/> without a name
/// or a named vector - array of <see cref="float"/> with associated name property.
/// </summary>
public abstract class SearchVector
{
    #region Nested classes

    /// <summary>
    /// Represents a search vector without a name. The single or default vector will be used to perform points search.
    /// </summary>
    internal sealed class UnnamedSearchVector : SearchVector
    {
        public float[] Vector { get; }

        public UnnamedSearchVector(float[] vector)
        {
            Vector = vector;
        }
    }

    /// <summary>
    /// Represents a named search vector. The vector with same name will be used to perform points search.
    /// </summary>
    internal sealed class NamedSearchVector : SearchVector
    {
        public string Name { get; }

        public float[] Vector { get; }

        public NamedSearchVector(string name, float[] vector)
        {
            Name = name;
            Vector = vector;
        }
    }

    #endregion

    /// <summary>
    /// Private ctor to enforce fatory methods usage.
    /// </summary>
    private SearchVector()
    { }

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for an unnamed vector.
    /// </summary>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(float[] vector) => new UnnamedSearchVector(vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for a named vector.
    /// </summary>
    /// <param name="vectorName">The name of the vector to use in search.</param>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(string vectorName, float[] vector) => new NamedSearchVector(vectorName, vector);

    /// <summary>
    /// Implicitly converts an array of <see cref="float"/> to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator SearchVector(float[] vector) => new UnnamedSearchVector(vector);

    /// <summary>
    /// Implicitly converts an instance of <see cref="VectorBase"/> to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator SearchVector(VectorBase vector)
    {
        if (vector is Vector v)
        {
            return new UnnamedSearchVector(v.VectorValues);
        }

        if (vector is NamedVectors { Vectors.Count: 1} nv)
        {
            if (nv.Vectors.Count == 1)
            {
                var firstVector = nv.Vectors.Single();

                return new NamedSearchVector(firstVector.Key, firstVector.Value);
            }
        }

        throw new InvalidCastException(
            $"Can't implicitly cast instance of type {vector.GetType()} to {typeof(SearchVector)}. "
            + $"The value should either be an unnamed vector or a named vectors collection with length of 1");
    }
}
