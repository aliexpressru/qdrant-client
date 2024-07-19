using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents sparse vectors configuration.
/// </summary>
[JsonConverter(typeof(SparseVectorConfigurationJsonConverter))]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class SparseVectorConfiguration
{
    /// <summary>
    /// Indicates whether to store sparse vector index on disk.
    /// If set to <c>false</c>, the index will be stored in RAM. Default: false.
    /// </summary>
    public bool OnDisk { set; get; }

    /// <summary>
    /// Indicates that index full scan should be employed for queries inspecting
    /// less than specified number of vectors. This upper bound is exclusive.
    /// </summary>
    /// <remarks>This is number of vectors, not KiloBytes.</remarks>
    public ulong? FullScanThreshold { set; get; }

    /// <summary>
    /// Defines which datatype should be used for the index.
    /// Choosing different datatypes allows to optimize memory usage and performance vs accuracy.
    /// </summary>
    public VectorDataType VectorDataType { set; get; }

    /// <summary>
    /// Configures additional value modifications for sparse vectors.
    /// </summary>
    public SparseVectorModifier Modifier { set; get; }

    /// <summary>
    /// Creates an instance of the sparse vector configuration with specified parameters.
    /// </summary>
    /// <param name="onDisk">Store index on disk. If set to <c>false</c>, the index will be stored in RAM. Default value is <c>false</c>.</param>
    /// <param name="fullScanThreshold">Prefer a full scan search upto (excluding) this number of vectors</param>
    /// <param name="vectorDataType">The vector data type.</param>
    /// <param name="sparseVectorValueModifier">The sparse vector value modifier.</param>
    public SparseVectorConfiguration(
        bool onDisk = false,
        ulong? fullScanThreshold = null,
        VectorDataType vectorDataType = VectorDataType.Float32,
        SparseVectorModifier sparseVectorValueModifier = SparseVectorModifier.None)
    {
        OnDisk = onDisk;
        FullScanThreshold = fullScanThreshold;
        VectorDataType = vectorDataType;
        Modifier = sparseVectorValueModifier;
    }
}
