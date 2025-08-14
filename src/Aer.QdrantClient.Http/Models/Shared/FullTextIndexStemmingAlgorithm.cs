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
    public class SnowballStemmingAlgorithm : FullTextIndexStemmingAlgorithm
    {
        /// <inheritdoc/>
        public sealed override StemmingAlgorithmType Type { init; get; }
        
        /// <summary>
        /// The language of the Snowball stemming algorithm.
        /// </summary>
        public SnowballStemmerLanguage Language { init; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextIndexStemmingAlgorithm.SnowballStemmingAlgorithm"/> class with the specified language.
        /// </summary>
        /// <param name="language">The Snowball stemming algorithm language.</param>
        public SnowballStemmingAlgorithm(SnowballStemmerLanguage language)
        {
            Type = StemmingAlgorithmType.Snowball;
            Language = language;
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FullTextIndexStemmingAlgorithm.SnowballStemmingAlgorithm"/> class with the specified language.
    /// </summary>
    /// <param name="language">The snowball stemming algorithm language.</param>
    public static FullTextIndexStemmingAlgorithm CreateSnowball(SnowballStemmerLanguage language) 
        => 
            new SnowballStemmingAlgorithm(language);
}
