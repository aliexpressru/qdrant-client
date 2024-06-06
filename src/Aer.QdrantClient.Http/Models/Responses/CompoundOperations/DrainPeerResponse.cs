using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of darin cluster node operation.
/// Note that drain node operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public class DrainPeerResponse : QdrantResponseBase<bool>
{ }
