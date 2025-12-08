using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// A base class for all Qdrant API responses.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public abstract class QdrantResponseBase
{
    /// <summary>
    /// Contains the string or object describing the operation status.
    /// </summary>
    [JsonConverter(typeof(QdrantStatusJsonConverter))]
    public QdrantStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the time (in seconds) elapsed for the operation.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Gets or sets the resource usage report for operation.
    /// Not all operations report usage data.
    /// </summary>
    public UsageReport Usage { get; set; }

    /// <summary>
    /// Represents the resource usage report if <c>service.hardware_reporting</c> config setting is set to <c>true</c>.
    /// <c>null</c> if it is <c>false</c> or missing.
    /// </summary>
    public class UsageReport
    {
        /// <summary>
        /// The hardware resources usage report. 
        /// </summary>
        public HardwareUsageReport Hardware { get; init; }

        /// <summary>
        /// CPU usage to execute request.
        /// </summary>
        public long Cpu
        {
            get => field == 0 && Hardware is not null
                ? Hardware.Cpu
                : field;

            set;
        }

        /// <summary>
        /// Payload IO read operations.
        /// </summary>
        public long PayloadIoRead
        {
            get => field == 0 && Hardware is not null
                ? Hardware.PayloadIoRead
                : field;

            set;
        }

        /// <summary>
        /// Payload IO write operations.
        /// </summary>
        public long PayloadIoWrite
        {
            get => field == 0 && Hardware is not null
                ? Hardware.PayloadIoWrite
                : field;

            set;
        }

        /// <summary>
        /// Vector IO read operations.
        /// </summary>
        public long VectorIoRead
        {
            get => field == 0 && Hardware is not null
                ? Hardware.VectorIoRead
                : field;

            set;
        }

        /// <summary>
        /// Vector IO write operations.
        /// </summary>
        public long VectorIoWrite
        {
            get => field == 0 && Hardware is not null
                ? Hardware.VectorIoWrite
                : field;

            set;
        }

        /// <summary>
        /// Represents an empty usage report.
        /// </summary>
        public static UsageReport Empty { get; } = new();

        /// <summary>
        /// Represents a hardware usage report.
        /// </summary>
        public class HardwareUsageReport
        {
            /// <summary>
            /// CPU usage to execute request.
            /// </summary>
            public long Cpu { get; init; }

            /// <summary>
            /// Payload IO read operations.
            /// </summary>
            public long PayloadIoRead { get; init; }

            /// <summary>
            /// Payload IO write operations.
            /// </summary>
            public long PayloadIoWrite { get; init; }

            /// <summary>
            /// Vector IO read operations.
            /// </summary>
            public long VectorIoRead { get; init; }

            /// <summary>
            /// Vector IO write operations.
            /// </summary>
            public long VectorIoWrite { get; init; }
        }
    }
}
