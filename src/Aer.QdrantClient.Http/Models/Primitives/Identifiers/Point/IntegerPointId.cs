using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Integer point identifier.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class IntegerPointId : PointId
{
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
    }

    /// <inheritdoc/>
    public override object ToJson()
        => Id;

    /// <inheritdoc/>
    public override ulong AsInteger() => Id;

    /// <inheritdoc/>
    public override Guid AsGuid()
        => throw new QdrantPointIdConversionException(GetType().FullName, typeof(Guid).FullName);

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

        if (other.GetType() != typeof(IntegerPointId))
        {
            return false;
        }

        return Id == ((IntegerPointId)other).Id;
    }

    /// <inheritdoc/>
    protected override int GetHashCodeCore()
        => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString()
        => Id.ToString();
}
