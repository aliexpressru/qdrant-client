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
}
