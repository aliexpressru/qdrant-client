using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the qdrant points batch operation response.
/// </summary>
public sealed class BatchPointsOperationResponse : QdrantResponseBase<QdrantOperationResult[]>
{ }
