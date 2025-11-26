using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents integer or string point identifier.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class SearchGroupId : IEquatable<SearchGroupId>
{
    #region Nested classes

    /// <summary>
    /// Integer search group identifier.
    /// </summary>
    private sealed class IntegerSearchGroupId : SearchGroupId
    {
        /// <summary>
        /// The identifier value.
        /// </summary>
        private readonly long _id;

        internal override object ObjectId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidPointId"/> class.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        public IntegerSearchGroupId(long id)
        {
            _id = id;
            ObjectId = _id.ToString();
        }

        /// <inheritdoc/>
        public override ulong AsInteger() =>
            throw new QdrantSearchGroupIdConversionException(GetType().FullName, typeof(int).FullName);

        /// <inheritdoc/>
        public override string AsString() => _id.ToString();

        /// <inheritdoc/>
        protected override bool EqualsCore(SearchGroupId other)
        {
            if (other is null)
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

            return _id == ((IntegerSearchGroupId)other)._id;
        }

        /// <inheritdoc/>
        protected override int GetHashCodeCore() => _id.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => _id.ToString();
    }

    /// <summary>
    /// string search group identifier.
    /// </summary>
    private sealed class StringSearchGroupId : SearchGroupId
    {
        /// <summary>
        /// The identifier value.
        /// </summary>
        private readonly string _id;

        internal override object ObjectId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidPointId"/> class.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        public StringSearchGroupId(string id)
        {
            _id = id;
            ObjectId = _id;
        }

        /// <inheritdoc/>
        public override ulong AsInteger() =>
            throw new QdrantSearchGroupIdConversionException(GetType().FullName, typeof(int).FullName);

        /// <inheritdoc/>
        public override string AsString() => _id;

        /// <inheritdoc/>
        protected override bool EqualsCore(SearchGroupId other)
        {
            if (other is null)
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

            return _id == ((StringSearchGroupId)other)._id;
        }

        /// <inheritdoc/>
        protected override int GetHashCodeCore()
            => _id.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => _id;
    }

    #endregion

    /// <summary>
    /// Gets the identifier in representation suitable for API request.
    /// </summary>
    internal abstract object ObjectId { get; }

    /// <summary>
    /// Gets the current SearchGroupId identifier value as integer. Throws if the SearchGroupId is not <see cref="IntegerSearchGroupId"/>.
    /// </summary>
    public abstract ulong AsInteger();

    /// <summary>
    /// Gets the current SearchGroupId identifier value as string.
    /// </summary>
    public abstract string AsString();

    #region Operators

    /// <summary>
    /// Performs an implicit conversion from <see cref="int"/> to <see cref="SearchGroupId"/>.
    /// </summary>
    /// <param name="id">The integer group identifier value.</param>
    public static implicit operator SearchGroupId(int id) => Integer(id);

    /// <summary>
    /// Performs an implicit conversion from <see cref="string"/> to <see cref="SearchGroupId"/>.
    /// </summary>
    /// <param name="id">The string group identifier value.</param>
    public static implicit operator SearchGroupId(string id) =>
        string.IsNullOrEmpty(id)
            ? throw new ArgumentNullException(nameof(id))
            : String(id);

    #endregion

    #region Factory methods

    /// <summary>
    /// Create instance of integer group identifier.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    public static SearchGroupId Integer(long groupId) =>
        groupId < 0
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
    public virtual bool Equals(SearchGroupId other) => EqualsCore(other);

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
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
