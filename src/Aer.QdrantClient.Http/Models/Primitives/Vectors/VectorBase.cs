using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a base class for all vector types.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class VectorBase
{
    /// <summary>
    /// The special default vector name.
    /// </summary>
    public const string DefaultVectorName = "default";

    /// <summary>
    /// Gets the kind of the vector that is represented by this <see cref="VectorBase"/> instance.
    /// </summary>
    [JsonIgnore]
    public abstract VectorKind VectorKind { get; }

    /// <summary>
    /// For <see cref="DenseVector"/> instance gets a vector itself,
    /// for <see cref="NamedVectors"/> gets the vector named <see cref="DefaultVectorName"/>
    /// for <see cref="SparseVector"/> throws an exception.
    /// for <see cref="MultiVector"/> gets the first vector component.
    /// </summary>
    [JsonIgnore]
    public abstract VectorBase Default { get; }

    /// <summary>
    /// Gets the named vector value if this instance is named vector collection
    /// or throws if it is the single vector instance.
    /// </summary>
    /// <param name="vectorName">Name of the vector to get.</param>
    public abstract VectorBase this[string vectorName] { get; }

    /// <summary>
    /// Returns the first vector from named vectors collection or a single from a single vector instance,
    /// for sparse vector throws na exception since sparse vectors are two-component values.
    /// </summary>
    public abstract VectorBase FirstOrDefault();

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
    /// Converts this instance into an instance of <see cref="DenseVector"/> type which represents a single unnamed vector.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a single unnamed vector.</exception>
    public DenseVector AsDenseVector() => this as DenseVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(DenseVector)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="SparseVector"/> type which represents a sparse vector.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a sparse vector.</exception>
    public SparseVector AsSparseVector() => this as SparseVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(SparseVector)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="MultiVector"/> type which represents a multivector.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a multivector.</exception>
    public MultiVector AsMultiVector() => this as MultiVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(MultiVector)}");

    /// <summary>
    /// Gets the string representation of this vector instance.
    /// </summary>
    public abstract override string ToString();

    /// <summary>
    /// Writes the string vector representation to the provided <see cref="StreamWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to write vector representation to.</param>
    public abstract void WriteToStream(StreamWriter writer);

    /// <summary>
    /// Writes the binary vector representation to the provided <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to write vector representation to.</param>
    public abstract void WriteToStream(BinaryWriter writer);

    /// <summary>
    /// Reads the vector representation from the provided <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="vectorKind">The kind of vector to read from stream.</param>
    /// <param name="reader">The reader to read vector representation from.</param>
    public static VectorBase ReadFromStream(
        VectorKind vectorKind,
        BinaryReader reader) 
        =>
        vectorKind switch
        {
            VectorKind.Dense => DenseVector.ReadFromStream(reader),
            VectorKind.Named => NamedVectors.ReadFromStream(reader),
            VectorKind.Sparse => SparseVector.ReadFromStream(reader),
            VectorKind.Multi => MultiVector.ReadFromStream(reader),
            _ => throw new ArgumentOutOfRangeException(nameof(vectorKind), vectorKind, $"Reading {vectorKind} vectors is not supported")
        };

    #region Operators

    /// <summary>
    /// Implicitly converts an array of <see cref="float"/> values to a vector instance.
    /// </summary>
    /// <param name="vectorValues">The vector values.</param>
    public static implicit operator VectorBase(float[] vectorValues)
    {
        if (vectorValues is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(vectorValues));
        }

        return new DenseVector()
        {
            VectorValues = vectorValues
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be <see cref="string"/>, dictionary value must be an array of <see cref="float"/>.
    /// </summary>
    /// <param name="namedVectors">The named vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, float[]> namedVectors)
    {
        if (namedVectors is null or {Count: 0})
        {
            throw new ArgumentNullException(nameof(namedVectors));
        }

        return new NamedVectors()
        {
            Vectors = namedVectors
                .ToDictionary(
                    nv => nv.Key,
                    nv => (VectorBase) new DenseVector()
                    {
                        VectorValues = nv.Value is null or {Length: 0}
                            ? throw new InvalidOperationException(
                                $"Can't create named vector {nv.Key} with null or empty vector value")
                            : nv.Value
                    })
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be <see cref="string"/>, dictionary value must be an array of <see cref="VectorBase"/>.
    /// </summary>
    /// <param name="namedVectors">The named vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, VectorBase> namedVectors)
    {
        if (namedVectors is null or {Count: 0})
        {
            throw new ArgumentNullException(nameof(namedVectors));
        }

        return new NamedVectors()
        {
            Vectors = namedVectors
                .ToDictionary(
                    nv => nv.Key,
                    nv => nv.Value)
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be a <see cref="string"/>, dictionary value must be an indices-values tuple.
    /// </summary>
    /// <param name="namedSparseVectors">The named sparse vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, (uint[] Indices, float[] Values)> namedSparseVectors)
    {
        if (namedSparseVectors is null or {Count: 0})
        {
            throw new ArgumentNullException(nameof(namedSparseVectors));
        }

        return new NamedVectors()
        {
            Vectors = namedSparseVectors
                .ToDictionary(
                    nv => nv.Key,
                    nv =>
                        (VectorBase) new SparseVector(nv.Value.Indices, nv.Value.Values)
                )
        };
    }

    /// <summary>
    /// Implicitly converts an indices-values tuple to a vector instance.
    /// Indices must be unique.
    /// Values and Indices must be the same length.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator VectorBase((uint[] Indices, float[] Values) sparseVectorComponents)
        => new SparseVector(sparseVectorComponents.Indices, sparseVectorComponents.Values);

    /// <summary>
    /// Implicitly converts an indices-values tuple to a vector instance.
    /// Indices must be unique.
    /// Values and Indices must be the same length.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator VectorBase((HashSet<uint> Indices, float[] Values) sparseVectorComponents)
        => new SparseVector(sparseVectorComponents.Indices, sparseVectorComponents.Values);

    /// <summary>
    /// Implicitly converts the <see cref="VectorBase"/> instance to an array of <see cref="float"/> values.
    /// Returns <see cref="Default"/> vector.
    /// </summary>
    /// <param name="vector">Instance to get single vector from.</param>
    public static explicit operator float[](VectorBase vector)
    {
        return vector switch
        {
            DenseVector denseVector => denseVector.VectorValues,
            MultiVector multiVector => multiVector.Default.AsDenseVector().VectorValues,
            NamedVectors namedVectors => (float[]) namedVectors.Default,
            SparseVector =>
                throw new NotSupportedException(
                    "Conversion from sparse vector to float[] is not supported since sparse vector is a multi-component value"),
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(vector))
        };
    }

    /// <summary>
    /// Implicitly converts a jagged array of floats to a multivector instance.
    /// </summary>
    /// <param name="multiVectorComponentVectors">The multivector component vectors.</param>
    public static implicit operator VectorBase(float[][] multiVectorComponentVectors)
        =>
            new MultiVector()
            {
                Vectors = multiVectorComponentVectors
            };

    #endregion
}
