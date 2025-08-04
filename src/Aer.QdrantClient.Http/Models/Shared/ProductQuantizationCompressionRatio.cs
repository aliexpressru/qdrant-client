// ReSharper disable InconsistentNaming | Justification = "Named as per Qdrant documentation"

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a product quantization compression ratio.
/// </summary>
public enum ProductQuantizationCompressionRatio
{
    /// <summary>
    /// X4 compression.
    /// </summary>
    x4,

    /// <summary>
    /// X8 compression.
    /// </summary>
    x8,

    /// <summary>
    /// X16 compression.
    /// </summary>
    x16,

    /// <summary>
    /// X32 compression.
    /// </summary>
    x32,

    /// <summary>
    /// X64 compression.
    /// </summary>
    x64
}
