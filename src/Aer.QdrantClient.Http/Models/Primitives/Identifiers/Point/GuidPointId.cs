using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// String point identifier.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GuidPointId"/> class.
/// </remarks>
/// <param name="id">The identifier value.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class GuidPointId(Guid id) : PointId, IEquatable<GuidPointId>, IComparable<GuidPointId>
{
    internal override object ObjectId { get; } = id.ToString();

    /// <summary>
    /// The identifier value.
    /// </summary>
    public Guid Id { get; } = id;

    /// <inheritdoc/>
    public override ulong AsInteger() =>
        throw new QdrantPointIdConversionException(GetType().FullName, typeof(int).FullName);

    /// <inheritdoc/>
    public override Guid AsGuid() => Id;

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

        if (other.GetType() != typeof(GuidPointId))
        {
            return false;
        }

        return Equals((GuidPointId)other);
    }

    /// <inheritdoc/>
    protected override int GetHashCodeCore() => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Id.ToString();

    /// <inheritdoc />
    public override string ToString(bool includeTypeInfo) =>
        includeTypeInfo
            ? $"Guid: {ToString()}"
            : ToString();

    /// <inheritdoc />
    public bool Equals(GuidPointId other)
    {
        if (other is null)
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <inheritdoc />
    public int CompareTo(GuidPointId other)
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
        return Equals(obj as GuidPointId);
    }

    /// <inheritdoc />
    public override int GetHashCode() => GetHashCodeCore();

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="GuidPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator GuidPointId(Guid id) => new(id);
}
