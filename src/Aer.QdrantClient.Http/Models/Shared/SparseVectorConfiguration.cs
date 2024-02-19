using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents sparse vectors configuration.
/// </summary>
[JsonConverter(typeof(SparseVectorConfigurationJsonConverter))]
public class SparseVectorConfiguration
{
    /// <summary>
    /// Indicates whether to store sparse vector index on disk.
    /// If set to <c>false</c>, the index will be stored in RAM. Default: false.
    /// </summary>
    public bool OnDisk { set; get; }

    /// <summary>
    /// Indicates that index full scan should be emplyed for queries inspecting
    /// less than specified number of vectors. This upper bound is exclusive.
    /// </summary>
    /// <remarks>This is number of vectors, not KiloBytes.</remarks>
    public ulong? FullScanThreshold { set; get; }

    /// <summary>
    /// Creates an instance of the sparse vector configuration with specified parameters.
    /// </summary>
    /// <param name="onDisk">Store index on disk. If set to <c>false</c>, the index will be stored in RAM. Default value is <c>false</c>.</param>
    /// <param name="fullScanThreshold">Prefer a full scan search upto (excluding) this number of vectors</param>
    public SparseVectorConfiguration(bool onDisk = false, ulong? fullScanThreshold = null)
    {
        OnDisk = onDisk;
        FullScanThreshold = fullScanThreshold;
    }
}
