using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the qdrant issues report response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ReportIssuesResponse : QdrantResponseBase<ReportIssuesResponse.QdrantIssuesUint>
{
    /// <summary>
    /// Represents a qdrant issues report.
    /// </summary>
    public sealed class QdrantIssuesUint
    {
        /// <summary>
        /// Reported qdrant issues.
        /// </summary>
        public KeyValuePair<string, string>[] Issues { set; get; }
    }
}
