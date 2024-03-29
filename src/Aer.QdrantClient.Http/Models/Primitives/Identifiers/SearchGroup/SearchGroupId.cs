using Aer.QdrantClient.Http.Exceptions;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents integer or string point identifier.
/// </summary>
public abstract class SearchGroupId : IEquatable<SearchGroupId>
{
    #region Nested classes

    /// <summary>
    /// Integer search group identifier.
    /// </summary>
    public class IntegerSearchGroupId : SearchGroupId
    {
        /// <summary>
        /// The identifier value.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidPointId"/> class.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        public IntegerSearchGroupId(long id)
        {
            Id = id;
        }

        /// <inheritdoc/>
        public override object ToJson()
            => Id.ToString();

        /// <inheritdoc/>
        public override ulong AsInteger()
            => throw new QdrantSearchGroupIdConversionException(GetType().FullName, typeof(int).FullName);

        /// <inheritdoc/>
        public override string AsString() => Id.ToString();

        /// <inheritdoc/>
        protected override bool EqualsCore(SearchGroupId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other.GetType() != typeof(IntegerSearchGroupId))
            {
                return false;
            }

            return Id == ((IntegerSearchGroupId) other).Id;
        }

        /// <inheritdoc/>
        protected override int GetHashCodeCore()
            => Id.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
            => Id.ToString();
    }

    /// <summary>
    /// string search group identifier.
    /// </summary>
    public class StringSearchGroupId : SearchGroupId
    {
        /// <summary>
        /// The identifier value.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidPointId"/> class.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        public StringSearchGroupId(string id)
        {
            Id = id;
        }

        /// <inheritdoc/>
        public override object ToJson() => Id;

        /// <inheritdoc/>
        public override ulong AsInteger()
            => throw new QdrantSearchGroupIdConversionException(GetType().FullName, typeof(int).FullName);

        /// <inheritdoc/>
        public override string AsString() => Id;

        /// <inheritdoc/>
        protected override bool EqualsCore(SearchGroupId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other.GetType() != typeof(StringSearchGroupId))
            {
                return false;
            }

            return Id == ((StringSearchGroupId) other).Id;
        }

        /// <inheritdoc/>
        protected override int GetHashCodeCore()
            => Id.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => Id;
    }

    #endregion

    /// <summary>
    /// Convert identifier to API request JSON representation.
    /// </summary>
    public abstract object ToJson();

    /// <summary>
    /// Gets the current SearchGroupId identifier value as integer. Throws if the SearchGroupId is not <see cref="IntegerSearchGroupId"/>.
    /// </summary>
    public abstract ulong AsInteger();

    /// <summary>
    /// Gets the current SearchGroupId identifier value as string.
    /// </summary>
    public abstract string AsString();

    #region Opeartors

    /// <summary>
    /// Performs an implicit conversion from <see cref="int"/> to <see cref="SearchGroupId"/>.
    /// </summary>
    /// <param name="id">The integer group identifier value.</param>
    public static implicit operator SearchGroupId(int id)
    {
        return Integer(id);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="SearchGroupId"/>.
    /// </summary>
    /// <param name="id">The string group identifier value.</param>
    public static implicit operator SearchGroupId(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return String(id);
    }

    #endregion

    #region Factory methods

    /// <summary>
    /// Create instance of integer group identifier.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    public static SearchGroupId Integer(long groupId)
        => groupId < 0
            ? throw new QdrantInvalidNumericSearchGroupIdException(groupId)
            : new IntegerSearchGroupId(groupId);

    /// <summary>
    /// Create instance of string group identifier.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    public static SearchGroupId String(string groupId) => new StringSearchGroupId(groupId);

    #endregion

    #region Equality members

    /// <summary>
    /// Determines whether <paramref name="other"/> equals this SearchGroupId.
    /// </summary>
    /// <param name="other">The other group identifier to compare this one with.</param>
    protected abstract bool EqualsCore(SearchGroupId other);

    /// <summary>
    /// Gets the hash code of this instance.
    /// </summary>
    /// <returns></returns>
    protected abstract int GetHashCodeCore();

    /// <inheritdoc/>
    public virtual bool Equals(SearchGroupId other)
        => EqualsCore(other);

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

        return Equals((SearchGroupId)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => GetHashCodeCore();

    #endregion
}
