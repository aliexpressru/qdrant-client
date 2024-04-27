using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Requests.Public;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if qdarnt batched operations request
/// such as <see cref="BatchUpdatePointsRequest"/> or <see cref="UpdateCollectionAliasesRequest"/> operations list is empty.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantEmptyBatchRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantEmptyBatchRequestException"/> class.
    /// </summary>
    /// <param name="collectionName">The collection invalid batched opertaion executed on.</param>
    /// <param name="operationName">The batched request operation name.</param>
    /// <param name="requestType">The type of the batched request.</param>
    public QdrantEmptyBatchRequestException(string collectionName, string operationName, Type requestType)
        : base($"Collection {collectionName} batch operation {operationName} request {requestType} has empty operations list. Add some operations to batched request using fluent interface")
    { }
}
