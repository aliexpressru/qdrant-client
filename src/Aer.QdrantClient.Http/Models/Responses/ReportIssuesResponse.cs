using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
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
        public QdrantIssue[] Issues { init; get; }

        /// <summary>
        /// Represents a single qdrant issue.
        /// </summary>
        public class QdrantIssue
        {
            /// <summary>
            /// Issue identifier.
            /// </summary>
            public string Id { init; get; }

            /// <summary>
            /// Issue description.
            /// </summary>
            public string Description { init; get; }

            /// <summary>
            /// The issue solution description if available.
            /// Parsed as a JsonObject for now since the api is not stabilized yet.
            /// See https://github.com/qdrant/qdrant/blob/master/lib/common/issues/src/solution.rs for details.
            /// </summary>
            public JsonObject Solution { init; get; }

            /// <summary>
            /// Gets or the issue timestamp.
            /// </summary>
            public DateTime Timestamp { init; get; }

            /// <summary>
            /// Gets the name of the collection this issue relates to if applicable.
            /// </summary>
            public string RelatedCollection { init; get; }
        }
    }
}
