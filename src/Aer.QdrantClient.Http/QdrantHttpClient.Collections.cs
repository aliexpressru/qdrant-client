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
        new HttpMethod("PATCH");
#else
        HttpMethod.Patch;
#endif

    /// <summary>
    /// Create collection.
    /// </summary>
    /// <param name="collectionName">Collection name. Must be maximum 255 characters long.</param>
    /// <param name="request">The collection creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    public async Task<DefaultOperationResponse> CreateCollection(
        string collectionName,
        CreateCollectionRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
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
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Update parameters of the existing collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to update.</param>
    /// <param name="request">Collection parameters to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout in seconds. If timeout is reached - request will return with service error.</param>
    public async Task<DefaultOperationResponse> UpdateCollectionParameters(
        string collectionName,
        UpdateCollectionParametersRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
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
            return await TriggerOptimizers(collectionName, cancellationToken);
        }

        var response = await ExecuteRequest<UpdateCollectionParametersRequest, DefaultOperationResponse>(
            url,
            _patchHttpMethod,
            request,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Trigger optimizers on existing collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout in seconds. If timeout is reached - request will return with service error.</param>
    /// <remarks>Issues the empty update collection parameters request to start optimizers for grey collections. https://qdrant.tech/documentation/concepts/collections/#grey-collection-status</remarks>
    public async Task<DefaultOperationResponse> TriggerOptimizers(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
    {
        EnsureQdrantNameCorrect(collectionName);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        var response = await ExecuteRequest<string, DefaultOperationResponse>(
            url,
            _patchHttpMethod,
            UpdateCollectionParametersRequest.EmptyRequestBody,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Get the detailed information about specified existing collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        CancellationToken cancellationToken)
    {
        EnsureQdrantNameCorrect(collectionName);

        var url = $"/collections/{collectionName}";

        var response = await ExecuteRequest<GetCollectionInfoResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Get the names of all the existing collections.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
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

    /// <summary>
    /// Delete collection by name.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    public async Task<DefaultOperationResponse> DeleteCollection(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
    {
        EnsureQdrantNameCorrect(collectionName);

        var url = $"/collections/{collectionName}?timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Get list of all existing collections aliases.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ListCollectionAliasesResponse> ListAllAliases(CancellationToken cancellationToken)
    {
        var url = "/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Get list of all aliases for a specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to list aliases for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ListCollectionAliasesResponse> ListCollectionAliases(
        string collectionName,
        CancellationToken cancellationToken)
    {
        var url = $"/collections/{collectionName}/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Execute multiple collection aliases update operations in one batch.
    /// </summary>
    /// <param name="updateCollectionAliasesRequest">The request with update aliases operations batch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    public async Task<DefaultOperationResponse> UpdateCollectionsAliases(
        UpdateCollectionAliasesRequest updateCollectionAliasesRequest,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
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
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Checks whether collection with specified name exists.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check existence for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
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
