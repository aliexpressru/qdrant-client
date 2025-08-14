namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The type of the text field tokenizer which determines how payload text will be split into words.
/// </summary>
public enum FullTextIndexTokenizerType
{
    /// <summary>
    /// Splits the string into words, separated by spaces, punctuation marks, and special characters,
    /// and then creates a prefix index for each word. For example: hello will be indexed as h, he, hel, hell, hello.
    /// </summary>
    Prefix,

    /// <summary>
    /// Splits the string into words, separated by spaces.
    /// </summary>
    Whitespace,

    /// <summary>
    /// Splits the string into words, separated by spaces, punctuation marks, and special characters.
    /// </summary>
    Word,

    /// <summary>
    /// A special type of tokenizer used to deliver fast and accurate tokenization for a large variety of languages. 
    /// </summary>
    Multilingual 
}
