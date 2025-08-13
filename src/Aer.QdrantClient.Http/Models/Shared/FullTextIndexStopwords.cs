namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Ignore this set of tokens. Can select from predefined languages and/or provide a custom set.
/// </summary>
public class FullTextIndexStopwords
{
    /// <summary>
    /// The custom stopwords set.
    /// </summary>
    public class CustomStopwordsSet : FullTextIndexStopwords
    { 
        /// <summary>
        /// The stopwords language.
        /// </summary>
        FullTextIndexStopwordsLanguage[] Languages { get; set; }
        
        /// <summary>
        /// The stopwords list.
        /// </summary>
        public string[] Custom { get; set; }
    }

    /// <summary>
    /// Default stopwords set for a specific language.
    /// </summary>
    public class DefaultStopwords : FullTextIndexStopwords
    {
        /// <summary>
        /// The stopwords language to get default stopwords for.
        /// </summary>
        public FullTextIndexStopwordsLanguage Language { get; set; }
    }
}
