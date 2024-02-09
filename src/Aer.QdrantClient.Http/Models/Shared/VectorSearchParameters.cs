namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the vector search parameters.
/// </summary>
public class VectorSearchParameters
{
    /// <summary>
    /// Params relevant to HNSW index. Size of the beam in a beam-search.
    /// Larger the value - more accurate the result, more time required for search.
    /// </summary>
    public uint? HnswEf { get; set; }

    /// <summary>
    /// Search without approximation. If set to true, search may run long but with exact results.
    /// </summary>
    public bool Exact { get; set; } = false;

    /// <summary>
    /// Quantization parameters.
    /// </summary>
    public QuantizationParameters Quantization { get; set; }

    /// <summary>
    /// If enabled, the engine will only perform search among indexed or small segments.
    /// Using this option prevents slow searches in case of delayed index,
    /// but does not guarantee that all uploaded vectors will be included in search results.
    /// </summary>
    public bool IndexedOnly { get; set; } = false;

    /// <summary>
    /// Represents quantization parameters.
    /// </summary>
    public class QuantizationParameters
    {
        /// <summary>
        /// If <c>true</c>, quantized vectors are ignored. Default is <c>false</c>.
        /// </summary>
        public bool Ignore { get; set; } = false;

        /// <summary>
        /// If <c>true</c>, use original vectors to re-score top-k results.
        /// Might require more time in case if original vectors are stored on disk. Default is <c>false</c>.
        /// </summary>
        public bool Rescore { get; set; } = false;

        /// <summary>
        /// Defines how many extra vectors should be pre-selected using quantized index,
        /// and then re-scored using original vectors.
        /// </summary>
        /// <remarks>
        /// For example, if oversampling is 2.4 and limit is 100, then 240 vectors will be pre-selected
        /// using quantized index, and then top-100 will be returned after re-scoring.
        /// </remarks>
        public double? Oversampling { get; set; }
    }
}
