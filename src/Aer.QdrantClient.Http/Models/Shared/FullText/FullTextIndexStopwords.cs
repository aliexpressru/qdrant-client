using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Ignore this set of tokens. Can select from predefined languages and/or provide a custom set.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class FullTextIndexStopwords
{
    /// <summary>
    /// The custom stopwords set.
    /// </summary>
    public sealed class CustomStopwordsSet : FullTextIndexStopwords
    {
        /// <summary>
        /// The stopwords language.
        /// </summary>
        public ICollection<StopwordsLanguage> Languages { get; init; }

        /// <summary>
        /// The stopwords list.
        /// </summary>
        public ICollection<string> Custom { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextIndexStopwords.CustomStopwordsSet"/> class with the specified languages and custom stopwords.
        /// </summary>
        /// <param name="languages">The custom stopwords list languages.</param>
        /// <param name="custom">The custom stopwords.</param>
        public CustomStopwordsSet(ICollection<StopwordsLanguage> languages, ICollection<string> custom)
        {
            Languages = languages;
            Custom = custom;
        }
    }

    /// <summary>
    /// Default stopwords set for a specific language.
    /// </summary>
    public sealed class DefaultStopwords : FullTextIndexStopwords
    {
        /// <summary>
        /// The stopwords language to get default stopwords for.
        /// </summary>
        public StopwordsLanguage Language { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextIndexStopwords.DefaultStopwords"/> class with the specified language.
        /// </summary>
        /// <param name="language">The default stopwords list language.</param>
        public DefaultStopwords(StopwordsLanguage language)
        {
            Language = language;
        }
    }

    /// <summary>
    /// Creates anew instance of the <see cref="FullTextIndexStopwords.CustomStopwordsSet"/> class with the specified languages and custom stopwords.
    /// </summary>
    /// <param name="languages">The custom stopwords list languages.</param>
    /// <param name="custom">The custom stopwords.</param>
    public static FullTextIndexStopwords CreateCustom(
        ICollection<StopwordsLanguage> languages,
        ICollection<string> custom)
        =>
            new CustomStopwordsSet(languages, custom);

    /// <summary>
    /// Creates a new instance of the <see cref="FullTextIndexStopwords.DefaultStopwords"/> class with the specified language.
    /// </summary>
    /// <param name="language">The default stopwords list language.</param>
    public static FullTextIndexStopwords CreateDefault(StopwordsLanguage language)
        =>
            new DefaultStopwords(language);
}
