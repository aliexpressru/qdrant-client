// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeProtected.Global

using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a base class for either a single vector or a colection of named vectors.
/// </summary>
public abstract class VectorBase
{
    /// <summary>
    /// The special default vector name.
    /// </summary>
    public const string DefaultVectorName = "default";

    /// <summary>
    /// For <see cref="Vector"/> instance gets a vector itself,
    /// for <see cref="NamedVectors"/> gets the vector named <see cref="DefaultVectorName"/>.
    /// </summary>
    [JsonIgnore]
    public abstract float[] Default { get; }

    /// <summary>
    /// <c>true</c> if this instance represents a named vector collection.
    /// <c>false</c> if this instance represents either a single unnamed vector or a sparse vector.
    /// </summary>
    [JsonIgnore]
    public bool IsNamedVectors => this is NamedVectors;

    /// <summary>
    /// <c>true</c> if this instance represents a sparse vector.
    /// <c>false</c> if this instance represents either a named vectors collection or a single unnamed vector.
    /// </summary>
    [JsonIgnore]
    public bool IsSparseVector => this is SparseVector;

    /// <summary>
    /// Gets the named vector if this instance is named vector collection or throws if it is the single vector instance.
    /// </summary>
    /// <param name="vectorName">Name of the vector to get.</param>
    public abstract float[] this[string vectorName] { get; }

    /// <summary>
    /// Returns the first vector from named vectors collection or a single from a single vector instance.
    /// </summary>
    public abstract float[] FirstOrDefault();

    /// <summary>
    /// Checks if named vectors for point contain vector with specified name.
    /// </summary>
    /// <param name="vectorName">Name to check.</param>
    /// <exception cref="InvalidOperationException">Occurs when the named vectors collection is empty.</exception>
    public abstract bool ContainsVector(string vectorName);

    /// <summary>
    /// Converts this instance into an instance of <see cref="NamedVectors"/> type which represents a multiple named vectors.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a multiple named vectors.</exception>
    public NamedVectors AsNamedVectors() => this as NamedVectors
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(NamedVectors)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="Vector"/> type which represents a single unnamed vector.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a single unnamed vector.</exception>
    public Vector AsSingleVector() => this as Vector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(Vector)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="SparseVector"/> type which represents a sparse vector.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a single unnamed vector.</exception>
    public SparseVector AsSparseVector() => this as SparseVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(SparseVector)}");

    #region Operators

    /// <summary>
    /// Implicitly converts an array of <see cref="float"/> values to a vector instance.
    /// </summary>
    /// <param name="vectorValues">The vector values.</param>
    public static implicit operator VectorBase(float[] vectorValues)
    {
        return new Vector()
        {
            VectorValues = vectorValues
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be <see cref="string"/>, dictionary value must be array of <see cref="float"/>.
    /// </summary>
    /// <param name="namedVectors">The named vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, float[]> namedVectors)
    {
        return new NamedVectors()
        {
            Vectors = namedVectors
                .ToDictionary(
                    nv => nv.Key,
                    nv => (VectorBase) new Vector()
                    {
                        VectorValues = nv.Value
                    })
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be a <see cref="string"/>, dictionary value must be an indices-values tuple.
    /// </summary>
    /// <param name="namedSparseVectors">The named sparse vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, (uint[] Indices, float[] Values)> namedSparseVectors)
    {
        return new NamedVectors()
        {
            Vectors = namedSparseVectors
                .ToDictionary(
                    nv => nv.Key,
                    nv => (VectorBase) new SparseVector()
                    {
                        Indices = nv.Value.Indices.ToHashSet(),
                        Values = nv.Value.Values
                    })
        };
    }

    /// <summary>
    /// Implicitly converts an indices-values tuple to a vector instance.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator VectorBase((uint[] Indices, float[] Values) sparseVectorComponents)
    {
        return new SparseVector()
        {
            Indices = sparseVectorComponents.Indices.ToHashSet(),
            Values = sparseVectorComponents.Values
        };
    }

    /// <summary>
    /// Implicitly converts the <see cref="VectorBase"/> instance to an array of <see cref="float"/> values.
    /// Returns <see cref="Default"/> vector.
    /// </summary>
    /// <param name="vector">Instance to get single vector from.</param>
    public static implicit operator float[](VectorBase vector)
    {
        return vector.Default;
    }

    #endregion
}
