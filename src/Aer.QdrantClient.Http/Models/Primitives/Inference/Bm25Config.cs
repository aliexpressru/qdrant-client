using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Inference;

/// <summary>
/// Represents a BM25 (Best Matching 25) inference model config.
/// </summary>
public sealed class Bm25Config : IEquatable<Bm25Config>
{
    /// <summary>
    /// Controls term frequency saturation.
    /// Higher values mean term frequency has more impact. Default is 1.2
    /// </summary>
    public double? K { get; init; } = 1.2;

    /// <summary>
    /// Controls document length normalization. Ranges from 0 (no normalization) to 1 (full normalization).
    /// Higher values mean longer documents have less impact. Default is 0.75.
    /// </summary>
    public double? B { get; init; } = 0.75;

    /// <summary>
    /// Expected average document length in the collection. Default is 256.
    /// </summary>
    public double? AvgLen { get; init; } = 256;

    /// <summary>
    /// The tokenizer to be used.
    /// </summary>
    public FullTextIndexTokenizerType Tokenizer { get; init; }

    /// <summary>
    /// Defines which language to use for text preprocessing.
    /// This parameter is used to construct default stopwords filter and stemmer.
    /// To disable language-specific processing, set this to "language": "none".
    /// If not specified, English is assumed.
    /// </summary>
    public string Language { get; init; }

    /// <summary>
    /// Lowercase the text before tokenization. Default is true.
    /// </summary>
    public bool Lowercase { get; init; } = true;

    /// <summary>
    /// If true, normalize tokens by folding accented characters to ASCII
    /// (e.g., “ação” -> “acao”). Default is false.
    /// </summary>
    public bool AsciiFolding { get; init; } = false;

    /// <summary>
    /// Configuration of the stemmer. Processes tokens to their root form.
    /// Default: initialized Snowball stemmer for specified language or English if not specified.
    /// </summary>
    [JsonConverter(typeof(FullTextIndexStemmingAlgorithmJsonConverter))]
    public FullTextIndexStemmingAlgorithm Stemmer { get; init; }

    /// <summary>
    /// Minimum token length to keep. If token is shorter than this, it will be discarded. Default no minimum length.
    /// </summary>
    public int? MinTokenLen { get; init; }

    /// <summary>
    /// Maximum token length to keep. If token is longer than this, it will be discarded. Default is no maximum length.
    /// </summary>
    public int? MaxTokenLen { get; init; }

    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as Bm25Config);

    /// <inheritdoc/>
    public bool Equals(Bm25Config other)
    {
        return other is not null
            && K == other.K
            && B == other.B
            && AvgLen == other.AvgLen
            && Tokenizer == other.Tokenizer
            && Language == other.Language
            && Lowercase == other.Lowercase
            && AsciiFolding == other.AsciiFolding
            && Stemmer.Equals(other.Stemmer)
            && MinTokenLen == other.MinTokenLen
            && MaxTokenLen == other.MaxTokenLen;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();

        hash.Add(K);
        hash.Add(B);
        hash.Add(AvgLen);
        hash.Add(Tokenizer);
        hash.Add(Language);
        hash.Add(Lowercase);
        hash.Add(AsciiFolding);
        hash.Add(Stemmer);
        hash.Add(MinTokenLen);
        hash.Add(MaxTokenLen);

        return hash.ToHashCode();
    }
}
