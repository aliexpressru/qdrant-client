using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents a grouped search lookup parameters.
/// </summary>
public class LookupSearchParameters
{
    /// <summary>
    /// The name of the collection to look up points in.
    /// </summary>
    public string CollectionName { set; get; }

    /// <summary>
    /// Options for specifying what to bring from the payload of the looked up point.
    /// </summary>
    [JsonConverter(typeof(PayloadPropertiesSelectorJsonConverter))]
    public PayloadPropertiesSelector WithPayload { set; get; }

    /// <summary>
    /// Options for specifying what to bring from the vector(s) of the looked up point.
    /// </summary>
    [JsonConverter(typeof(VectorSelectorJsonConverter))]
    public VectorSelector WithVectors { set; get; }
}
