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
    /// Inidicates whether the sparse vector confuration should stay on disk.
    /// </summary>
    public bool OnDisk { set; get; }

    /// <summary>
    /// Indicates that index full scan should be emplyed for queries inspecting
    /// less than specified number of vectors.
    /// </summary>
    public ulong? FullScanThreshold { set; get; }

    /// <summary>
    /// Creates an instance of the sparse vector configuration with specified parameters.
    /// </summary>
    public SparseVectorConfiguration(bool onDisk, ulong? fullScanThreshold = null)
    {
        OnDisk = onDisk;
        FullScanThreshold = fullScanThreshold;
    }
}
