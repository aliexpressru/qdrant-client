﻿using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the collection quantization configuration.
/// </summary>
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ReplaceAutoPropertyWithComputedProperty")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class QuantizationConfiguration
{
    #region Quantization types

    internal class ScalarQuantizationConfiguration : QuantizationConfiguration
    {
        public const string QuantizationMethodName = "scalar";

        /// <summary>
        /// The type of the quantized vector components. Currently, Qdrant supports only <c>int8</c>.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public string Type { get; } = "int8";

        /// <summary>
        /// The quantile of the quantized vector components. The quantile is used to calculate the quantization bounds.
        /// </summary>
        /// <remarks>
        /// For instance, if you specify 0.99 as the quantile, 1% of extreme values will be excluded from the quantization bounds.
        /// Using quantiles lower than 1.0 might be useful if there are outliers in your vector components.
        /// This parameter only affects the resulting precision and not the memory footprint.
        /// It might be worth tuning this parameter if you experience a significant decrease in search quality.
        /// </remarks>
        public float? Quantile { set; get; }

        /// <summary>
        /// Whether to keep quantized vectors always cached in RAM or not.
        /// </summary>
        /// <remarks>By default, quantized vectors are loaded in the same way as the original vectors.
        /// However, in some setups you might want to keep quantized vectors in RAM to speed up the search process.
        /// Then set always_ram to <c>true</c>.
        /// <br/>
        /// <b>There are 3 possible modes to place storage of vectors within the qdrant collection:</b>
        /// <br/>
        /// <ul>
        /// <li>
        /// <c>All in RAM</c> - all vector, original and quantized, are loaded and kept in RAM.
        /// This is the fastest mode, but requires a lot of RAM. Enabled by default.
        /// </li>
        /// <li>
        /// <c>Original on Disk</c>, quantized in RAM - this is a hybrid mode, allows to obtain a good balance between speed
        /// and memory usage. Recommended scenario if you are aiming to shrink the memory footprint
        /// while keeping the search speed. This mode is enabled by setting AlwaysRam to <c>true</c> in the quantization config while using memmap storage
        /// </li>
        /// <li>
        /// <c>All on Disk</c> - all vectors, original and quantized, are stored on disk.
        /// This mode allows to achieve the smallest memory footprint, but at the cost of the search speed.
        /// It is recommended to use this mode if you have a large collection and fast storage (e.g. SSD or NVMe).
        /// This mode is enabled by setting AlwaysRam to <c>false</c> in the quantization config while using memmap storage
        /// </li>
        /// </ul>
        /// </remarks>
        public bool AlwaysRam { set; get; }
    }

    internal class ProductQuantizationConfiguration : QuantizationConfiguration
    {
        public const string QuantizationMethodName = "product";

        /// <summary>
        /// Compression ratio. Compression ratio represents the size of the quantized vector in bytes
        /// divided by the size of the original vector in bytes.
        /// </summary>
        public ProductQuantizationCompressionRatio Compression { set; get; }

        /// <summary>
        /// Whether to keep quantized vectors always cached in RAM or not.
        /// </summary>
        /// <remarks>By default, quantized vectors are loaded in the same way as the original vectors.
        /// However, in some setups you might want to keep quantized vectors in RAM to speed up the search process.
        /// Then set always_ram to <c>true</c>.
        /// <br/>
        /// <b>There are 3 possible modes to place storage of vectors within the qdrant collection:</b>
        /// <br/>
        /// <ul>
        /// <li>
        /// <c>All in RAM</c> - all vector, original and quantized, are loaded and kept in RAM.
        /// This is the fastest mode, but requires a lot of RAM. Enabled by default.
        /// </li>
        /// <li>
        /// <c>Original on Disk</c>, quantized in RAM - this is a hybrid mode, allows to obtain a good balance between speed
        /// and memory usage. Recommended scenario if you are aiming to shrink the memory footprint
        /// while keeping the search speed. This mode is enabled by setting AlwaysRam to <c>true</c> in the quantization config while using memmap storage
        /// </li>
        /// <li>
        /// <c>All on Disk</c> - all vectors, original and quantized, are stored on disk.
        /// This mode allows to achieve the smallest memory footprint, but at the cost of the search speed.
        /// It is recommended to use this mode if you have a large collection and fast storage (e.g. SSD or NVMe).
        /// This mode is enabled by setting AlwaysRam to <c>false</c> in the quantization config while using memmap storage
        /// </li>
        /// </ul>
        /// </remarks>
        public bool AlwaysRam { set; get; } = false;
    }

    internal class BinaryQuantizationConfiguration : QuantizationConfiguration
    {
        public const string QuantizationMethodName = "binary";

        /// <summary>
        /// Whether to keep quantized vectors always cached in RAM or not.
        /// </summary>
        /// <remarks>By default, quantized vectors are loaded in the same way as the original vectors.
        /// However, in some setups you might want to keep quantized vectors in RAM to speed up the search process.
        /// Then set always_ram to <c>true</c>.
        /// <br/>
        /// <b>There are 3 possible modes to place storage of vectors within the qdrant collection:</b>
        /// <br/>
        /// <ul>
        /// <li>
        /// <c>All in RAM</c> - all vector, original and quantized, are loaded and kept in RAM.
        /// This is the fastest mode, but requires a lot of RAM. Enabled by default.
        /// </li>
        /// <li>
        /// <c>Original on Disk</c>, quantized in RAM - this is a hybrid mode, allows to obtain a good balance between speed
        /// and memory usage. Recommended scenario if you are aiming to shrink the memory footprint
        /// while keeping the search speed. This mode is enabled by setting AlwaysRam to <c>true</c> in the quantization config while using memmap storage
        /// </li>
        /// <li>
        /// <c>All on Disk</c> - all vectors, original and quantized, are stored on disk.
        /// This mode allows to achieve the smallest memory footprint, but at the cost of the search speed.
        /// It is recommended to use this mode if you have a large collection and fast storage (e.g. SSD or NVMe).
        /// This mode is enabled by setting AlwaysRam to <c>false</c> in the quantization config while using memmap storage
        /// </li>
        /// </ul>
        /// </remarks>
        public bool AlwaysRam { set; get; } = false;
    }

    #endregion

    /// <summary>
    /// Creates a scalar quantization configuration.
    /// </summary>
    /// <param name="quantile">The quantile of the quantized vector components.</param>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    public static QuantizationConfiguration Scalar(
        float? quantile = null,
        bool isQuantizedVectorAlwaysInRam = false
    )
        =>
            new ScalarQuantizationConfiguration()
            {
                Quantile = quantile,
                AlwaysRam = isQuantizedVectorAlwaysInRam
            };

    /// <summary>
    /// Creates a product quantization configuration.
    /// </summary>
    /// <param name="quantizedVectorsCompressionRatio">Vector compression ratio.</param>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    public static QuantizationConfiguration Product(
        ProductQuantizationCompressionRatio quantizedVectorsCompressionRatio,
        bool isQuantizedVectorAlwaysInRam = false)
        =>
            new ProductQuantizationConfiguration()
            {
                Compression = quantizedVectorsCompressionRatio,
                AlwaysRam = isQuantizedVectorAlwaysInRam
            };

    /// <summary>
    /// Creates a binary quantization configuration.
    /// </summary>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    public static QuantizationConfiguration Binary(bool isQuantizedVectorAlwaysInRam = false)
        =>
            new BinaryQuantizationConfiguration()
            {
                AlwaysRam = isQuantizedVectorAlwaysInRam
            };
}
