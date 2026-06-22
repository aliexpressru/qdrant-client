using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the collection quantization configuration diff for partial collection updates.
/// </summary>
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ReplaceAutoPropertyWithComputedProperty")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class QuantizationConfigurationDiff
{
    #region Quantization types

    /// <summary>
    /// The quantization method.
    /// </summary>
    public abstract string Method { get; }

    /// <summary>
    /// Represents the scalar quantization configuration diff.
    /// </summary>
    public sealed class ScalarQuantizationConfigurationDiff : QuantizationConfigurationDiff
    {
        internal const string QuantizationMethodName = "scalar";

        /// <inheritdoc/>
        public override string Method => QuantizationMethodName;

        /// <summary>
        /// The type of the quantized vector components. Currently, Qdrant supports only <c>int8</c>.
        /// </summary>
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
        public bool? AlwaysRam { set; get; }
    }

    /// <summary>
    /// Represents the product quantization configuration diff.
    /// </summary>
    public sealed class ProductQuantizationConfigurationDiff : QuantizationConfigurationDiff
    {
        internal const string QuantizationMethodName = "product";

        /// <inheritdoc/>
        public override string Method => QuantizationMethodName;

        /// <summary>
        /// Compression ratio. Compression ratio represents the size of the quantized vector in bytes
        /// divided by the size of the original vector in bytes.
        /// </summary>
        [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<ProductQuantizationCompressionRatio>))]
        public ProductQuantizationCompressionRatio Compression { set; get; }

        /// <summary>
        /// Whether to keep quantized vectors always cached in RAM or not.
        /// </summary>
        public bool? AlwaysRam { set; get; }
    }

    /// <summary>
    /// Represents the binary quantization configuration diff.
    /// </summary>
    public sealed class BinaryQuantizationConfigurationDiff : QuantizationConfigurationDiff
    {
        internal const string QuantizationMethodName = "binary";

        /// <inheritdoc/>
        public override string Method => QuantizationMethodName;

        /// <summary>
        /// Whether to keep quantized vectors always cached in RAM or not.
        /// </summary>
        public bool? AlwaysRam { set; get; }

        /// <summary>
        /// The bit depth of the quantized vector components.
        /// </summary>
        [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<BinaryQuantizationEncoding>))]
        public BinaryQuantizationEncoding? Encoding { set; get; }

        /// <summary>
        /// Asymmetric quantization configuration.
        /// Asymmetric quantization configuration allows a query to have different quantization than stored vectors.
        /// It can increase the accuracy of search at the cost of performance.
        /// </summary>
        [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<BinaryQuantizationQueryEncoding>))]
        public BinaryQuantizationQueryEncoding? QueryEncoding { set; get; }
    }

    /// <summary>
    /// Represents the TurboQuant quantization configuration diff.
    /// TurboQuant uses asymmetric quantization automatically: only stored vectors are compressed, while queries are scored in full precision.
    /// </summary>
    /// <remarks>
    /// TurboQuant fully supports Cosine, Dot, and Euclidean (L2) distance with SIMD-accelerated scoring.
    /// Manhattan(L1) distance is supported but requires full vector reconstruction per comparison, making it significantly slower than the other metrics.
    /// Use Cosine, Dot, or Euclidean distance for best performance with TurboQuant.
    /// </remarks>
    public sealed class TurboQuantizationConfigurationDiff : QuantizationConfigurationDiff
    {
        internal const string QuantizationMethodName = "turbo";

        /// <inheritdoc/>
        public override string Method => QuantizationMethodName;

        /// <summary>
        /// Whether to keep quantized vectors always cached in RAM or not.
        /// </summary>
        public bool? AlwaysRam { set; get; }

        /// <summary>
        /// The encoding bit depth. Defaults to <see cref="TurboQuantizationEncoding.Bits4"/>. Lower bit depths offer higher compression at the cost of accuracy.
        /// </summary>
        [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<TurboQuantizationEncoding>))]
        public TurboQuantizationEncoding? Bits { set; get; }
    }

    /// <summary>
    /// Represents the disabled quantization configuration. Use to remove quantization from the collection.
    /// </summary>
    public sealed class DisabledQuantizationConfigurationDiff : QuantizationConfigurationDiff
    {
        internal const string QuantizationMethodName = "disabled";

        /// <inheritdoc/>
        public override string Method => QuantizationMethodName;
    }

    #endregion

    /// <summary>
    /// Creates a scalar quantization configuration diff.
    /// </summary>
    /// <param name="quantile">The quantile of the quantized vector components.</param>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    public static QuantizationConfigurationDiff Scalar(float? quantile = null, bool isQuantizedVectorAlwaysInRam = false) =>
        new ScalarQuantizationConfigurationDiff() { Quantile = quantile, AlwaysRam = isQuantizedVectorAlwaysInRam };

    /// <summary>
    /// Creates a product quantization configuration diff.
    /// </summary>
    /// <param name="quantizedVectorsCompressionRatio">Vector compression ratio.</param>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    public static QuantizationConfigurationDiff Product(
        ProductQuantizationCompressionRatio quantizedVectorsCompressionRatio,
        bool isQuantizedVectorAlwaysInRam = false
    ) =>
        new ProductQuantizationConfigurationDiff()
        {
            Compression = quantizedVectorsCompressionRatio,
            AlwaysRam = isQuantizedVectorAlwaysInRam,
        };

    /// <summary>
    /// Creates a binary quantization configuration diff.
    /// </summary>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    /// <param name="encoding">The quantization bit depth.</param>
    /// <param name="queryEncoding">The asymmetric quantization configuration.</param>
    public static QuantizationConfigurationDiff Binary(
        bool isQuantizedVectorAlwaysInRam = false,
        BinaryQuantizationEncoding? encoding = null,
        BinaryQuantizationQueryEncoding? queryEncoding = null
    ) =>
        new BinaryQuantizationConfigurationDiff()
        {
            AlwaysRam = isQuantizedVectorAlwaysInRam,
            Encoding = encoding,
            QueryEncoding = queryEncoding,
        };

    /// <summary>
    /// Creates a turbo quantization configuration diff.
    /// </summary>
    /// <param name="isQuantizedVectorAlwaysInRam">Whether to keep quantized vectors always cached in RAM or not.</param>
    /// <param name="bits">The encoding bit depth.</param>
    public static QuantizationConfigurationDiff Turbo(
        bool isQuantizedVectorAlwaysInRam = false,
        TurboQuantizationEncoding? bits = null
    ) => new TurboQuantizationConfigurationDiff() { AlwaysRam = isQuantizedVectorAlwaysInRam, Bits = bits };

    /// <summary>
    /// Creates a disabled quantization configuration diff. Removes quantization from the collection.
    /// </summary>
    public static QuantizationConfigurationDiff Disabled() => new DisabledQuantizationConfigurationDiff();
}
