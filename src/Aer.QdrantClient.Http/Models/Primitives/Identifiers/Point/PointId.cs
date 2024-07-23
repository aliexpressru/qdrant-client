using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents integer or string point identifier.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class PointId : IEquatable<PointId>
{
    /// <summary>
    /// Convert identifier to API request JSON representation.
    /// </summary>
    public abstract object ToJson();

    /// <summary>
    /// Gets the current PointId identifier value as integer. Throws if the PointId is not <see cref="IntegerPointId"/>.
    /// </summary>
    public abstract ulong AsInteger();

    /// <summary>
    /// Gets the current PointId identifier value as Guid. Throws if the PointId is not <see cref="GuidPointId"/>.
    /// </summary>
    public abstract Guid AsGuid();

    /// <summary>
    /// Gets the current PointId identifier value as string.
    /// </summary>
    public abstract string AsString();

    #region Opeartors

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="PointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator PointId(ulong id)
    {
        return Integer(id);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="int"/> to <see cref="PointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator PointId(int id)
    {
        return Integer(id);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="System.Guid"/> to <see cref="PointId"/>.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    public static implicit operator PointId(Guid id)
    {
        return Guid(id);
    }

    #endregion

    #region Factory methods

    /// <summary>
    /// Create instance of integer point identifier.
    /// </summary>
    /// <param name="pointId">The point identifier.</param>
    public static PointId Integer(ulong pointId) => new IntegerPointId(pointId);

    /// <summary>
    /// Create instance of integer point identifier.
    /// </summary>
    /// <param name="pointId">The point identifier.</param>
    public static PointId Integer(int pointId)
        => pointId < 0
            ? throw new QdrantInvalidNumericPointIdException(pointId)
            : new IntegerPointId((ulong) pointId);

    /// <summary>
    /// Create instance of integer point identifier.
    /// </summary>
    /// <param name="pointId">The point identifier.</param>
    public static PointId Integer(long pointId)
        => pointId < 0
            ? throw new QdrantInvalidNumericPointIdException(pointId)
            : new IntegerPointId((ulong) pointId);

    /// <summary>
    /// Create instance of GUID point identifier.
    /// </summary>
    /// <param name="pointId">The point identifier.</param>
    public static PointId Guid(Guid pointId) => new GuidPointId(pointId);

    /// <summary>
    /// Create instance of GUID point identifier from string GUID representation.
    /// Throws if the provided string is not parseable as GUID.
    /// </summary>
    /// <param name="pointId">The point identifier.</param>
    public static PointId Guid(string pointId) => new GuidPointId(System.Guid.Parse((ReadOnlySpan<char>)pointId));

    /// <summary>
    /// Create instance of GUID point identifier with new random guid.
    /// </summary>
    public static PointId NewGuid() => new GuidPointId(System.Guid.NewGuid());

    #endregion

    #region Equality members

    /// <summary>
    /// Determines whether <paramref name="other"/> equals this PointId.
    /// </summary>
    /// <param name="other">The other point to compare this one with/</param>
    protected abstract bool EqualsCore(PointId other);

    /// <summary>
    /// Gets the hash code of this instance.
    /// </summary>
    /// <returns></returns>
    protected abstract int GetHashCodeCore();

    /// <inheritdoc/>
    public virtual bool Equals(PointId other)
        => EqualsCore(other);

    /// <summary>
    /// Determines whether point id <paramref name="x"/> equals point id <paramref name="y"/>.
    /// </summary>
    /// <param name="x">First point id to compare.</param>
    /// <param name="y">Second point id to compare.</param>
    public static bool operator ==(PointId x, PointId y)
    {
        if (x is null
            && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            // here x or y is not null
            return false;
        }

        return x.Equals(y);
    }

    /// <summary>
    /// Determines whether point id <paramref name="x"/> is not equal to the point id <paramref name="y"/>.
    /// </summary>
    /// <param name="x">First point id to compare.</param>
    /// <param name="y">Second point id to compare.</param>
    public static bool operator !=(PointId x, PointId y)
    {
        if (x is null
            && y is null)
        {
            return false;
        }

        if (x is null
            || y is null)
        {
            // here x or y is not null
            return true;
        }

        return !x.Equals(y);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((PointId)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => GetHashCodeCore();
    #endregion
}
