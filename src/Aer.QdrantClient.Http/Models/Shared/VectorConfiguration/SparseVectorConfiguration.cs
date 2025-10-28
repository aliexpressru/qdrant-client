using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents sparse vectors configuration.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class SparseVectorConfiguration
{
    /// <summary>
    /// Configures additional value modifications for sparse vectors.
    /// </summary>
    public SparseVectorModifier Modifier { set; get; } = SparseVectorModifier.None;
    
    /// <summary>
    /// Custom params for index. If none - values from collection configuration are used.
    /// </summary>
    public SparseVectorIndexConfiguration Index { set; get; } = new();
    
    /// <summary>
    /// Custom sparse vector index parameters.
    /// </summary>
    public class SparseVectorIndexConfiguration
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
        /// </summary>
        public VectorDataType Datatype { set; get; } = VectorDataType.Float32;
    }

    /// <summary>
    /// Creates an instance of the sparse vector configuration with default parameters.
    /// </summary>
    public SparseVectorConfiguration()
    { }

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
        Modifier = sparseVectorValueModifier;

        Index = new()
        {
            OnDisk = onDisk,
            FullScanThreshold = fullScanThreshold,
            Datatype = vectorDataType
        };
    }
}
