using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The available bit depths for binary quantization.
/// The bit depth determines how many bits are used to represent each value in the quantized binary vector.
/// The higher the bit depth, the more precise the representation, but also the larger the size of the quantized vector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public enum BinaryQuantizationEncoding
{
    /// <summary>
    /// 1bit quantization encoding.
    /// </summary>
    OneBit,
    
    /// <summary>
    /// 2bit quantization encoding.
    /// </summary>
    TwoBits,
    
    /// <summary>
    /// 1.5bit quantization encoding.
    /// </summary>
    OneAndHalfBits,
}
