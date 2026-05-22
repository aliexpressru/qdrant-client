namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The available bit depths for turbo quantization.
/// The bit depth determines how many bits are used to represent each value in the quantized vector.
/// The higher the bit depth, the more precise the representation, but also the larger the size of the quantized vector.
/// </summary>
public enum TurboQuantizationEncoding
{
    /// <summary>
    /// 1bit encoding.
    /// </summary>
    Bits1,

    /// <summary>
    /// 1.5 bit encoding.
    /// </summary>
    Bits1_5,

    /// <summary>
    /// 2 bit encoding.
    /// </summary>
    Bits2,

    /// <summary>
    /// 4 bit encoding.
    /// </summary>
    Bits4,
}
