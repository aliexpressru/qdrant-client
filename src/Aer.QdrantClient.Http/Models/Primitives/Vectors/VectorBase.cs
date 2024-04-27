using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a base class for either a single vector or a collection of named vectors.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class VectorBase
{
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
    public bool IsSparseVector => this is SparseFloatVector;

    /// <summary>
    /// A data type used to represent this vector values.
    /// </summary>
    [JsonIgnore]
    public VectorDataType DataType {
        get
        {
            return this switch
            {
                FloatVector => VectorDataType.Float32,
                SparseFloatVector => VectorDataType.Float32,
                ByteVector => VectorDataType.Uint8,
                SparseByteVector => VectorDataType.Uint8,
                _ => throw new InvalidOperationException($"Vector of type {GetType()} has an unknown data type")
            };

        }
    }

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
    /// Converts this instance into an instance of <see cref="FloatVector"/> type which represents a single unnamed vector of float32 values.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a single unnamed float32 vector.</exception>
    public FloatVector AsFloatVector() => this as FloatVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(FloatVector)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="SparseFloatVector"/> type which represents a sparse vector of float32 values.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a sparse float32 vector.</exception>
    public SparseFloatVector AsSparseFloatVector() => this as SparseFloatVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(SparseFloatVector)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="ByteVector"/> type which represents a single unnamed vector of byte values.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a single unnamed byte vector.</exception>
    public ByteVector AsByteVector() => this as ByteVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(ByteVector)}");

    /// <summary>
    /// Converts this instance into an instance of <see cref="SparseByteVector"/> type which represents a sparse vector of byte values.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs when this instance is not a sparse byte vector.</exception>
    public SparseByteVector AsSparseByteVector() => this as SparseByteVector
        ?? throw new InvalidCastException($"Can't convert instance of {GetType()} to {typeof(SparseFloatVector)}");

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

        return new FloatVector()
        {
            Values = vectorValues
        };
    }

    /// <summary>
    /// Implicitly converts an array of <see cref="byte"/> values to a vector instance.
    /// </summary>
    /// <param name="vectorValues">The vector values.</param>
    public static implicit operator VectorBase(byte[] vectorValues)
    {
        if (vectorValues is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(vectorValues));
        }

        return new ByteVector()
        {
            Values = vectorValues
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be <see cref="string"/>, dictionary value must be array of <see cref="float"/>.
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
                    nv => (VectorBase) new FloatVector()
                    {
                        Values = nv.Value is null or {Length: 0}
                            ? throw new InvalidOperationException(
                                $"Can't create named vector {nv.Key} with null or empty vector value")
                            : nv.Value
                    })
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be <see cref="string"/>, dictionary value must be array of <see cref="byte"/>.
    /// </summary>
    /// <param name="namedVectors">The named vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, byte[]> namedVectors)
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
                    nv => (VectorBase) new ByteVector()
                    {
                        Values = nv.Value is null or {Length: 0}
                            ? throw new InvalidOperationException(
                                $"Can't create named vector {nv.Key} with null or empty vector value")
                            : nv.Value
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
        if (namedSparseVectors is null or {Count: 0})
        {
            throw new ArgumentNullException(nameof(namedSparseVectors));
        }

        return new NamedVectors()
        {
            Vectors = namedSparseVectors
                .ToDictionary(
                    nv => nv.Key,
                    nv => (VectorBase) new SparseFloatVector()
                    {
                        Indices = nv.Value.Indices?.ToHashSet(),
                        Values = nv.Value.Values
                    })
        };
    }

    /// <summary>
    /// Implicitly converts a dictionary of type <see cref="Dictionary{TKey,TValue}"/> to a vector instance.
    /// Dictionary key must be a <see cref="string"/>, dictionary value must be an indices-values tuple.
    /// </summary>
    /// <param name="namedSparseVectors">The named sparse vectors.</param>
    public static implicit operator VectorBase(Dictionary<string, (uint[] Indices, byte[] Values)> namedSparseVectors)
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
                    nv => (VectorBase) new SparseByteVector()
                    {
                        Indices = nv.Value.Indices?.ToHashSet(),
                        Values = nv.Value.Values
                    })
        };
    }

    /// <summary>
    /// Implicitly converts an indices-values tuple to a vector instance.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator VectorBase((uint[] Indices, float[] Values) sparseVectorComponents)
        =>
            new SparseFloatVector()
            {
                Indices = sparseVectorComponents.Indices?.ToHashSet(),
                Values = sparseVectorComponents.Values
            };

    /// <summary>
    /// Implicitly converts an indices-values tuple to a vector instance.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator VectorBase((uint[] Indices, byte[] Values) sparseVectorComponents)
        =>
            new SparseByteVector()
            {
                Indices = sparseVectorComponents.Indices?.ToHashSet(),
                Values = sparseVectorComponents.Values
            };

    #endregion
}
