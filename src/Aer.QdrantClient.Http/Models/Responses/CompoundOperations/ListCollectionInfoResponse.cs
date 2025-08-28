using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the list collection information operation response.
/// </summary>
public sealed class ListCollectionInfoResponse : QdrantResponseBase<Dictionary<string, GetCollectionInfoResponse.CollectionInfo>>
{ }
