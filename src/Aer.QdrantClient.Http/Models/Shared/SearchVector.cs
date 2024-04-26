using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a vector that used as search request. Either an array of <see cref="float"/> without a name
/// or a named vector - array of <see cref="float"/> with associated name property.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class SearchVector
{
    #region Nested classes

    /// <summary>
    /// Represents a search vector without a name.
    /// The single or default vector will be used to perform points search.
    /// </summary>
    internal sealed class UnnamedFloatSearchVector : SearchVector
    {
        public float[] Vector { get; }

        public UnnamedFloatSearchVector(float[] vector)
        {
            Vector = vector;
        }
    }

    /// <summary>
    /// Represents a search vector without a name.
    /// The single or default vector will be used to perform points search.
    /// </summary>
    internal sealed class UnnamedByteSearchVector : SearchVector
    {
        public byte[] Vector { get; }

        public UnnamedByteSearchVector(byte[] vector)
        {
            Vector = vector;
        }
    }

    /// <summary>
    /// Represents a named search vector. The vector with same name will be used to perform points search.
    /// </summary>
    internal sealed class NamedFloatSearchVector : SearchVector
    {
        public string Name { get; }

        public float[] Vector { get; }

        public NamedFloatSearchVector(string name, float[] vector)
        {
            Name = name;
            Vector = vector;
        }
    }

    /// <summary>
    /// Represents a named search vector. The vector with same name will be used to perform points search.
    /// </summary>
    internal sealed class NamedByteSearchVector : SearchVector
    {
        public string Name { get; }

        public byte[] Vector { get; }

        public NamedByteSearchVector(string name, byte[] vector)
        {
            Name = name;
            Vector = vector;
        }
    }

    #endregion

    /// <summary>
    /// Private ctor to enforce factory methods usage.
    /// </summary>
    private SearchVector()
    { }

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for an unnamed float32 vector.
    /// </summary>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(float[] vector) => new UnnamedFloatSearchVector(vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for an unnamed byte vector.
    /// </summary>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(byte[] vector) => new UnnamedByteSearchVector(vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for a named float32 vector.
    /// </summary>
    /// <param name="vectorName">The name of the vector to use in search.</param>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(string vectorName, float[] vector) => new NamedFloatSearchVector(vectorName, vector);

    /// <summary>
    /// Creates a <see cref="SearchVector"/> for a named byte vector.
    /// </summary>
    /// <param name="vectorName">The name of the vector to use in search.</param>
    /// <param name="vector">The vector to use in search.</param>
    public static SearchVector Create(string vectorName, byte[] vector) => new NamedByteSearchVector(vectorName, vector);

    #region Operators

    /// <summary>
    /// Implicitly converts an array of <see cref="float"/> to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator SearchVector(float[] vector)
    {
        if (vector is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(vector));
        }

        return new UnnamedFloatSearchVector(vector);
    }

    /// <summary>
    /// Implicitly converts an array of <see cref="byte"/> to an instance of <see cref="SearchVector"/>.
    /// </summary>
    /// <param name="vector">The value to convert.</param>
    public static implicit operator SearchVector(byte[] vector)
    {
        if (vector is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(vector));
        }

        return new UnnamedByteSearchVector(vector);
    }

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
            case FloatVector fv:
                return new UnnamedFloatSearchVector(fv.Values);
            case ByteVector bv:
                return new UnnamedByteSearchVector(bv.Values);
            case NamedVectors { Vectors.Count: 1 } nv:
            {
                var firstVector = nv.Vectors.Single();

                return firstVector.Value.DataType switch{
                    VectorDataType.Float32 => new NamedFloatSearchVector(firstVector.Key, firstVector.Value.AsFloatVector().Values),
                    VectorDataType.Uint8 => new NamedByteSearchVector(firstVector.Key, firstVector.Value.AsByteVector().Values),
                    _ => throw new InvalidOperationException($"Can't implicitly convert a vector with data type {firstVector.Value.DataType} to an instance of {nameof(SearchVector)}")
                };
            }
            default:
                throw new InvalidCastException(
                    $"Can't implicitly cast instance of type {vector.GetType()} to {typeof(SearchVector)}. "
                    + $"The value should either be an unnamed vector or a named vectors collection with length of 1");
        }
    }

    #endregion
}
