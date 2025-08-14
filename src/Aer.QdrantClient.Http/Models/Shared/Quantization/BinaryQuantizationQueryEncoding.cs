using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the binary quantization query encoding.
/// Used to set up asymmetric quantization.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Enum values are used in API and should match the API specification.")]
public enum BinaryQuantizationQueryEncoding
{
    /// <summary>
    /// 
    /// </summary>
    Default,
    
    /// <summary>
    /// Binary query quantization.
    /// </summary>
    Binary,
    
    /// <summary>
    /// Scalar quantization with 4 bits per dimension.
    /// </summary>
    Scalar4bits,
    
    /// <summary>
    /// Scalar quantization with 8 bits per dimension.
    /// </summary>
    Scalar8bits,
}
