using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a cluster node emptiness check result.
/// </summary>
public class CheckIsPeerEmptyResponse : QdrantResponseBase<bool>
{ }
