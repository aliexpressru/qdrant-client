using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http;

/// <summary>
/// Interface for Qdrant HTTP API client.
/// </summary>
public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Creates the full text index on specified payload text field.
    /// </summary>
    /// <remarks>
    /// For indexing, it is recommended to choose the field that limits the search result the most.
    /// As a rule, the more different values a payload value has, the more efficient the index will be used.
    /// You should not create an index for Boolean fields and fields with only a few possible values.
    /// </remarks>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="payloadTextFieldName">Name of the indexed payload text field.</param>
    /// <param name="payloadTextFieldTokenizerType">Type of the payload text field tokenizer.</param>
    /// <param name="minimalTokenLength">The minimal word token length.</param>
    /// <param name="maximalTokenLength">The maximal word token length.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isLowercasePayloadTokens">If <c>true</c>, lowercase all tokens. Default: <c>true</c>.</param>
    /// <param name="onDisk">
    /// If set to <c>true</c> the payload will be stored on-disk instead of in-memory.
    /// On-disk payload index might affect cold requests latency, as it requires additional disk I/O operations.
    /// </param>
    /// <param name="enablePhraseMatching">Enable phrase matching on this text field.</param>
    /// <param name="stemmer">Algorithm for stemming. If <c>null</c> stemming is disabled.</param>
    /// <param name="stopwords">Ignore this set of tokens. Can select from predefined languages and/or provide a custom set.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<PayloadIndexOperationResponse> CreateFullTextPayloadIndex(
        string collectionName,
        string payloadTextFieldName,
        FullTextIndexTokenizerType payloadTextFieldTokenizerType,
        CancellationToken cancellationToken,

        uint? minimalTokenLength,
        uint? maximalTokenLength,

        bool isLowercasePayloadTokens,
        bool onDisk,
        bool enablePhraseMatching,

        FullTextIndexStemmingAlgorithm stemmer,
        FullTextIndexStopwords stopwords,

        bool isWaitForResult,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Deletes the index for a payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="fieldName">Name of the field to delete index for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<PayloadIndexOperationResponse> DeletePayloadIndex(
        string collectionName,
        string fieldName,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

}
