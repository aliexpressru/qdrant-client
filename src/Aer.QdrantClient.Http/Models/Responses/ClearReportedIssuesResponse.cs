using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the qdrant issues clear response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ClearReportedIssuesResponse : QdrantResponseBase<bool>
{ }
