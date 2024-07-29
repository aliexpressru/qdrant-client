using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// String point identifier.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class GuidPointId : PointId
{
    internal override object ObjectId { get; }

    /// <summary>
    /// The identifier value.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuidPointId"/> class.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public GuidPointId(Guid id)
    {
        Id = id;
        ObjectId = id.ToString();
    }

    /// <inheritdoc/>
    public override ulong AsInteger()
        => throw new QdrantPointIdConversionException(GetType().FullName, typeof(int).FullName);

    /// <inheritdoc/>
    public override Guid AsGuid() => Id;

    /// <inheritdoc/>
    public override string AsString() => Id.ToString();

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

        if (other.GetType() != typeof(GuidPointId))
        {
            return false;
        }

        return Id == ((GuidPointId)other).Id;
    }

    /// <inheritdoc/>
    protected override int GetHashCodeCore()
        => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString()
        => Id.ToString();
}
