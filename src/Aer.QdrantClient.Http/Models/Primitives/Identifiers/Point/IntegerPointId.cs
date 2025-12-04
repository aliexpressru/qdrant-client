using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Integer point identifier.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IntegerPointId"/> class.
/// </remarks>
/// <param name="id">The identifier value.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class IntegerPointId(ulong id) : PointId, IEquatable<IntegerPointId>, IComparable<IntegerPointId>
{
    internal override object ObjectId { get; } = id;

    /// <summary>
    /// The identifier value.
    /// </summary>
    public ulong Id { get; } = id;

    /// <inheritdoc/>
    public override ulong AsInteger() => Id;

    /// <inheritdoc/>
    public override Guid AsGuid() =>
        throw new QdrantPointIdConversionException(GetType().FullName, typeof(Guid).FullName);

    /// <inheritdoc/>
    protected override bool EqualsCore(PointId other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetType() != typeof(IntegerPointId))
        {
            return false;
        }

        return Equals((IntegerPointId)other);
    }

    /// <inheritdoc/>
    protected override int GetHashCodeCore() => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Id.ToString();

    /// <inheritdoc />
    public override string ToString(bool includeTypeInfo)
        =>
            includeTypeInfo
                ? $"Int: {ToString()}"
                : ToString();

    /// <inheritdoc />
    public bool Equals(IntegerPointId other)
    {
        if (other is null)
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <inheritdoc />
    public int CompareTo(IntegerPointId other)
    {
        if (other is null)
        {
            return 1;
        }

        return Id.CompareTo(other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as IntegerPointId);
    }

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCodeCore();

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="IntegerPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator IntegerPointId(ulong id) => new(id);

    /// <summary>
    /// Performs an implicit conversion from <see cref="long"/> to <see cref="IntegerPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator IntegerPointId(long id) =>
        id < 0
            ? throw new QdrantInvalidPointIdException(id)
            : new IntegerPointId((ulong)id);

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="IntegerPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator IntegerPointId(uint id) => new(id);

    /// <summary>
    /// Performs an implicit conversion from <see cref="int"/> to <see cref="IntegerPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator IntegerPointId(int id) =>
        id < 0
            ? throw new QdrantInvalidPointIdException(id)
            : new IntegerPointId((ulong)id);
}
