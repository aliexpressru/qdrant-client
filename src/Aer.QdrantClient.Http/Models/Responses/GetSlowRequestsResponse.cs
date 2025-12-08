using Aer.QdrantClient.Http.Models.Responses.Base;
using System.Text.Json.Nodes;
using static Aer.QdrantClient.Http.Models.Responses.GetSlowRequestsResponse;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant slow requests profiler.
/// </summary>
public sealed class GetSlowRequestsResponse : QdrantResponseBase<SlowRequestsData>
{
    /// <summary>
    /// Represents a collection of slow request records as reported by Qdrant.
    /// </summary>
    public sealed class SlowRequestsData
    {
        /// <summary>
        /// List of slow requests recorded by Qdrant.
        /// </summary>
        public SlowRequestInfo[] Requests { get; set; }
    }

    /// <summary>
    /// Represents information about a slow request recorded by Qdrant.
    /// </summary>
    public sealed class SlowRequestInfo
    {
        /// <summary>
        /// The collection name where the slow request occurred.
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// The slow request duration in seconds.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// The date and time when the slow request was made.
        /// </summary>
        public DateTime Datetime { get; set; }

        /// <summary>
        /// The name of the slow request operation.
        /// </summary>
        public string RequestName { get; set; }

        /// <summary>
        /// Approximate number of times this request has been recorded.
        /// </summary>
        public uint ApproxCount { get; set; }

        /// <summary>
        /// The slow request body.
        /// </summary>
        public JsonObject RequestBody { get; set; }
    }
}
