using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    private readonly HashSet<PayloadIndexedFieldType> _allowedPayloadFieldTypesForTenantIndex =
    [
        PayloadIndexedFieldType.Keyword,
        PayloadIndexedFieldType.Uuid
    ];

    private readonly HashSet<PayloadIndexedFieldType> _allowedPayloadFieldTypesForPrincipalIndex =
    [
        PayloadIndexedFieldType.Integer,
        PayloadIndexedFieldType.Float,
        PayloadIndexedFieldType.Datetime
    ];

    /// <inheritdoc/>
    public async Task<PayloadIndexOperationResponse> CreatePayloadIndex(
        string collectionName,
        string payloadFieldName,
        PayloadIndexedFieldType payloadFieldType,
        CancellationToken cancellationToken,
        bool isWaitForResult = false,
        bool onDisk = false,

        bool? isTenant = null,
        bool? isPrincipal = null,

        bool? isLookupEnabled = null,
        bool? isRangeEnabled = null,

        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);
        EnsureQdrantNameCorrect(payloadFieldName);

        if (isTenant.HasValue
            && isTenant.Value
            && !_allowedPayloadFieldTypesForTenantIndex.Contains(payloadFieldType))
        {
            throw new QdrantUnsupportedFieldSchemaForIndexConfiguration(
                $"Tenant index is not supported for payload field {payloadFieldName} with type {payloadFieldType}. Supported types: [{string.Join(", ", _allowedPayloadFieldTypesForTenantIndex)}]");
        }

        if (isPrincipal.HasValue
            && isPrincipal.Value
            && !_allowedPayloadFieldTypesForPrincipalIndex.Contains(payloadFieldType))
        {
            throw new QdrantUnsupportedFieldSchemaForIndexConfiguration(
                $"Principal index is not supported for payload field {payloadFieldName} with type {payloadFieldType}. Supported types: [{string.Join(", ", _allowedPayloadFieldTypesForPrincipalIndex)}]");
        }

        if (isLookupEnabled.HasValue
            && isLookupEnabled.Value
            && payloadFieldType != PayloadIndexedFieldType.Integer)
        {
            throw new QdrantUnsupportedFieldSchemaForIndexConfiguration(
                $"Lookup index is only supported for payload field {payloadFieldName} with type {PayloadIndexedFieldType.Integer}");
        }

        if (isRangeEnabled.HasValue
            && isRangeEnabled.Value
            && payloadFieldType != PayloadIndexedFieldType.Integer)
        {
            throw new QdrantUnsupportedFieldSchemaForIndexConfiguration(
                $"Range index is only supported for payload field {payloadFieldName} with type {PayloadIndexedFieldType.Integer}");
        }

        if (payloadFieldType == PayloadIndexedFieldType.Text)
        {
            throw new InvalidOperationException(
                $"Direct creation of the text type indexes (fulltext search indexes) is not supported. To create fulltext index please use {nameof(CreateFullTextPayloadIndex)} method");
        }

        var createIndexRequest = new CreatePayloadIndexRequest(
            payloadFieldName,
            payloadFieldType,
            onDisk: onDisk,
            isTenant: isTenant,
            isPrincipal: isPrincipal,

            isLookupEnabled: payloadFieldType == PayloadIndexedFieldType.Integer
                ? isLookupEnabled ?? true
                : null,
            isRangeFilterEnabled: payloadFieldType == PayloadIndexedFieldType.Integer
                ? isRangeEnabled ?? true
                : null
        );

        var url = $"/collections/{collectionName}/index?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreatePayloadIndexRequest, PayloadIndexOperationResponse>(
            url,
            HttpMethod.Put,
            createIndexRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
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

        bool? isAsciiFoldingEnabled = null,

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
                Stopwords = stopwords,
                AsciiFolding = isAsciiFoldingEnabled
            });

        var url = $"/collections/{collectionName}/index?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateFullTextPayloadIndexRequest, PayloadIndexOperationResponse>(
            url,
            HttpMethod.Put,
            createIndexRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
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
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }
}
