using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    private static readonly HttpMethod _patchHttpMethod =
#if NETSTANDARD2_0
        new("PATCH");
#else
        HttpMethod.Patch;
#endif

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> CreateCollection(
        string collectionName,
        CreateCollectionRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureQdrantNameCorrect(collectionName);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        var response = await ExecuteRequest<CreateCollectionRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> UpdateCollectionParameters(
        string collectionName,
        UpdateCollectionParametersRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureQdrantNameCorrect(collectionName);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        if (request.IsEmpty)
        {
            // Empty request is swapped with trigger optimizers request.
            return await TriggerOptimizers(
                collectionName,
                cancellationToken,
                timeout,
                retryCount,
                retryDelay,
                onRetry);
        }

        var response = await ExecuteRequest<UpdateCollectionParametersRequest, DefaultOperationResponse>(
            url,
            _patchHttpMethod,
            request,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> TriggerOptimizers(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        var response = await ExecuteRequest<string, DefaultOperationResponse>(
            url,
            _patchHttpMethod,
            UpdateCollectionParametersRequest.EmptyRequestBody,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);

        var url = $"/collections/{collectionName}";

        var response = await ExecuteRequest<GetCollectionInfoResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionsResponse> ListCollections(CancellationToken cancellationToken)
    {
        var url = "/collections";

        var response = await ExecuteRequest<ListCollectionsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteCollection(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        EnsureQdrantNameCorrect(collectionName);

        var url = $"/collections/{collectionName}?timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionAliasesResponse> ListAllAliases(
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var url = "/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionAliasesResponse> ListCollectionAliases(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var url = $"/collections/{collectionName}/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> UpdateCollectionsAliases(
        UpdateCollectionAliasesRequest updateCollectionAliasesRequest,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        if (updateCollectionAliasesRequest.OperationsCount == 0)
        {
            throw new QdrantEmptyBatchRequestException(
                "N/A",
                nameof(UpdateCollectionsAliases),
                updateCollectionAliasesRequest.GetType());
        }

        var url = $"/collections/aliases?timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<UpdateCollectionAliasesRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Post,
            updateCollectionAliasesRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<CheckCollectionExistsResponse> CheckCollectionExists(
        string collectionName,
        CancellationToken cancellationToken)
    {
        var url = $"/collections/{collectionName}/exists";

        var response = await ExecuteRequest<CheckCollectionExistsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }
}
