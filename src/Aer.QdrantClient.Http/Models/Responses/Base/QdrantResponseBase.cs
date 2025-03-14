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
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
    /// Gets or sets the hardware usage report for operation.
    /// Not all operations report usage data.
    /// </summary>
    public HardwareUsageReport Usage { get; set; }

    /// <summary>
    /// Represents the resource usage report if <c>service.hardware_reporting</c> config setting is set to <c>true</c>.
    /// <c>null</c> if it is <c>false</c> or missing.
    /// </summary>
    public class HardwareUsageReport
    { 
        /// <summary>
        /// Cpu usage to execute request.
        /// </summary>
        public long Cpu { get; set; }
        
        /// <summary>
        /// Payload IO read operations.
        /// </summary>
        public long PayloadIoRead { get; set; }
        
        /// <summary>
        /// Payload IO write operations.
        /// </summary>
        public long PayloadIoWrite { get; set; }
        
        /// <summary>
        /// Vector IO read operations.
        /// </summary>
        public long VectorIoRead { get; set; }
        
        /// <summary>
        /// Vector IO write operations.
        /// </summary>
        public long VectorIoWrite { get; set; }
    }
}
