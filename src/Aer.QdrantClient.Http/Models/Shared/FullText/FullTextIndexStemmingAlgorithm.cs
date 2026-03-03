namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a fulltext stemmer algorithm definition.
/// </summary>
public abstract class FullTextIndexStemmingAlgorithm
{
    /// <summary>
    /// The type of the stemming algorithm.
    /// </summary>
    public abstract StemmingAlgorithmType Type { init; get; }

    /// <summary>
    /// The snowball stemming algorithm.
    /// </summary>
    public sealed class SnowballStemmingAlgorithm : FullTextIndexStemmingAlgorithm
    {
        /// <inheritdoc/>
        public override StemmingAlgorithmType Type { init; get; }

        /// <summary>
        /// The language of the Snowball stemming algorithm.
        /// </summary>
        public SnowballStemmerLanguage Language { init; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowballStemmingAlgorithm"/> class with the specified language.
        /// </summary>
        /// <param name="language">The Snowball stemming algorithm language.</param>
        public SnowballStemmingAlgorithm(SnowballStemmerLanguage language)
        {
            Type = StemmingAlgorithmType.Snowball;
            Language = language;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            HashCode hashCode = new();

            hashCode.Add(Type);
            hashCode.Add(Language);

            return hashCode.ToHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is SnowballStemmingAlgorithm sa
                && sa.Language.Equals(Language)
                && sa.Type.Equals(Type);
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SnowballStemmingAlgorithm"/> class with the specified language.
    /// </summary>
    /// <param name="language">The snowball stemming algorithm language.</param>
    public static FullTextIndexStemmingAlgorithm CreateSnowball(SnowballStemmerLanguage language)
        =>
            new SnowballStemmingAlgorithm(language);

    /// <inheritdoc/>
    public abstract override int GetHashCode();

    /// <inheritdoc/>
    public abstract override bool Equals(object other);
}
