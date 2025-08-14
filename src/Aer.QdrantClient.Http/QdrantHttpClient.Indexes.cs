using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    private readonly HashSet<PayloadIndexedFieldType> _allowedPayloadFieldTypesForTenantIndex = [
        PayloadIndexedFieldType.Keyword,
        PayloadIndexedFieldType.Uuid
    ];

    private readonly HashSet<PayloadIndexedFieldType> _allowedPayloadFieldTypesForPrincipalIndex = [
        PayloadIndexedFieldType.Integer,
        PayloadIndexedFieldType.Float,
        PayloadIndexedFieldType.Datetime
    ];

    /// <summary>
    /// Creates an index on specified payload field.
    /// </summary>
    /// <remarks>
    /// For indexing, it is recommended to choose the field that limits the search result the most.
    /// As a rule, the more different values a payload value has, the more efficient the index will be used.
    /// You should not create an index for Boolean fields and fields with only a few possible values.
    /// </remarks>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="payloadFieldName">Name of the indexed payload field.</param>
    /// <param name="payloadFieldType">Type of the indexed payload field.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="onDisk">
    /// If set to <c>true</c> the payload will be stored on-disk instead of in-memory.
    /// On-disk payload index might affect cold requests latency, as it requires additional disk I/O operations.
    /// </param>
    /// <param name="isTenant">Set to <c>true</c> to enable tenant index for specified payload field.</param>
    /// <param name="isPrincipal">
    /// Set to <c>true</c> to enable principal index for specified payload field.
    /// The principal index is used to optimize storage for faster search,
    /// assuming that the search request is primarily filtered by the principal field.
    /// </param>
    /// <remarks>
    /// Payload index on-disk storage is currently supported for the following types:
    /// <see cref="PayloadIndexedFieldType.Keyword"/>,
    /// <see cref="PayloadIndexedFieldType.Integer"/>,
    /// <see cref="PayloadIndexedFieldType.Float"/>,
    /// <see cref="PayloadIndexedFieldType.Datetime"/>,
    /// <see cref="PayloadIndexedFieldType.Uuid"/>,
    /// <see cref="PayloadIndexedFieldType.Text"/>,
    /// <see cref="PayloadIndexedFieldType.Geo"/>.
    ///
    /// Tenant optimization is supported for the following datatypes:
    /// <see cref="PayloadIndexedFieldType.Keyword"/>,
    /// <see cref="PayloadIndexedFieldType.Uuid"/>
    ///
    /// Principal optimization is supported for following types:
    /// <see cref="PayloadIndexedFieldType.Integer"/>,
    /// <see cref="PayloadIndexedFieldType.Float"/>,
    /// <see cref="PayloadIndexedFieldType.Datetime"/>
    /// </remarks>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    public async Task<PayloadIndexOperationResponse> CreatePayloadIndex(
        string collectionName,
        string payloadFieldName,
        PayloadIndexedFieldType payloadFieldType,
        CancellationToken cancellationToken,
        bool isWaitForResult = false,
        bool onDisk = false,
        bool isTenant = false,
        bool isPrincipal = false,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);
        EnsureQdrantNameCorrect(payloadFieldName);

        if (isTenant && !_allowedPayloadFieldTypesForTenantIndex.Contains(payloadFieldType))
        {
            throw new QdrantUnsupportedFieldSchemaForIndexConfiguration(
                $"Tenant index is not supported for payload field {payloadFieldName} with type {payloadFieldType}. Supported types: [{string.Join(", ", _allowedPayloadFieldTypesForTenantIndex)}]");
        }

        if (isPrincipal && !_allowedPayloadFieldTypesForPrincipalIndex.Contains(payloadFieldType))
        {
            throw new QdrantUnsupportedFieldSchemaForIndexConfiguration(
                $"Principal index is not supported for payload field {payloadFieldName} with type {payloadFieldType}. Supported types: [{string.Join(", ", _allowedPayloadFieldTypesForPrincipalIndex)}]");
        }

        if (payloadFieldType == PayloadIndexedFieldType.Text)
        {
            throw new InvalidOperationException(
                $"Direct creation of the text type indexes (fulltext search indexes) is not supported. To create fulltext index please use {nameof(CreateFullTextPayloadIndex)} method");
        }

        var index = new CreatePayloadIndexRequest(
            payloadFieldName, 
            payloadFieldType, 
            onDisk: onDisk, 
            isTenant: isTenant, 
            isPrincipal: isPrincipal);

        var url = $"/collections/{collectionName}/index?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreatePayloadIndexRequest, PayloadIndexOperationResponse>(
            url,
            HttpMethod.Put,
            index,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

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
    public async Task<PayloadIndexOperationResponse> CreateFullTextPayloadIndex(
        string collectionName,
        string payloadTextFieldName,
        FullTextIndexTokenizerType payloadTextFieldTokenizerType,
        CancellationToken cancellationToken,
        
        uint? minimalTokenLength = null,
        uint? maximalTokenLength = null,
        
        bool isLowercasePayloadTokens = true,
        bool onDisk = false,
        bool enablePhraseMatching = false,
        
        FullTextIndexStemmingAlgorithm stemmer = null,
        FullTextIndexStopwords stopwords = null,
        
        bool isWaitForResult = false,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);
        EnsureQdrantNameCorrect(payloadTextFieldName);

        var createIndexRequest = new CreateFullTextPayloadIndexRequest(
            payloadTextFieldName,
            new CreateFullTextPayloadIndexRequest.FullTextPayloadFieldSchema()
            {
                Tokenizer = payloadTextFieldTokenizerType,
                MinTokenLen = minimalTokenLength,
                MaxTokenLen = maximalTokenLength,
                Lowercase = isLowercasePayloadTokens,
                OnDisk = onDisk,
                PhraseMatching = enablePhraseMatching,
                Stemmer = stemmer,
                Stopwords = stopwords
            });

        var url = $"/collections/{collectionName}/index?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateFullTextPayloadIndexRequest, PayloadIndexOperationResponse>(
            url,
            HttpMethod.Put,
            createIndexRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

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
    public async Task<PayloadIndexOperationResponse> DeletePayloadIndex(
        string collectionName,
        string fieldName,
        CancellationToken cancellationToken,
        bool isWaitForResult = false,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);
        EnsureQdrantNameCorrect(fieldName);

        var url = $"/collections/{collectionName}/index/{fieldName}?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<PayloadIndexOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }
}
