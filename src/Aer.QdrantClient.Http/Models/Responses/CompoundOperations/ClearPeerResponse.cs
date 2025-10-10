using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of a clear cluster node operation.
/// Note that clear node operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public sealed class ClearPeerResponse : QdrantResponseBase<bool>
{ }
