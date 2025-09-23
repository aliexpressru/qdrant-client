using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Integer point identifier.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class IntegerPointId : PointId
{
    internal override object ObjectId { get; }

    /// <summary>
    /// The identifier value.
    /// </summary>
    public ulong Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerPointId"/> class.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public IntegerPointId(ulong id)
    {
        Id = id;
        ObjectId = id;
    }

    /// <inheritdoc/>
    public override ulong AsInteger() => Id;

    /// <inheritdoc/>
    public override Guid AsGuid()
        => 
            throw new QdrantPointIdConversionException(GetType().FullName, typeof(Guid).FullName);

    /// <inheritdoc/>
    protected override bool EqualsCore(PointId other)
    {
        if (ReferenceEquals(null, other))
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

        return Id == ((IntegerPointId)other).Id;
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

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="IntegerPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator IntegerPointId(ulong id) => new (id);

    /// <summary>
    /// Performs an implicit conversion from <see cref="long"/> to <see cref="IntegerPointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator IntegerPointId(long id) =>
        id < 0
            ? throw new QdrantInvalidPointIdException(id)
            : new IntegerPointId((ulong) id);

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
            : new IntegerPointId((ulong) id);
}
