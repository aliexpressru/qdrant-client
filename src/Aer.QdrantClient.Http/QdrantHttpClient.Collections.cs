using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;

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
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(CreateCollection), null);

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
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(UpdateCollectionParameters), null);

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
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(TriggerOptimizers), null);

        EnsureQdrantNameCorrect(collectionName);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        var response = await ExecuteRequest<string, DefaultOperationResponse>(
            url,
            _patchHttpMethod,
            UpdateCollectionParametersRequest.EmptyRequestBody,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetCollectionInfo), clusterName);

        EnsureQdrantNameCorrect(collectionName);

        var url = $"/collections/{collectionName}";

        var response = await ExecuteRequest<GetCollectionInfoResponse>(
            url,
            HttpMethod.Get,
            clusterName ?? collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionsResponse> ListCollections(CancellationToken cancellationToken, string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ListCollections), clusterName);

        var url = "/collections";

        var response = await ExecuteRequest<ListCollectionsResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteCollection), null);

        EnsureQdrantNameCorrect(collectionName);

        var url = $"/collections/{collectionName}?timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionAliasesResponse> ListAllAliases(
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DeleteCollection), clusterName);

        var url = "/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteCollection), null);

        var url = $"/collections/{collectionName}/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> UpdateCollectionsAliases(
        UpdateCollectionAliasesRequest updateCollectionAliasesRequest,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DeleteCollection), clusterName);

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
            clusterName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CheckCollectionExistsResponse> CheckCollectionExists(
        string collectionName,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteCollection), null);

        var url = $"/collections/{collectionName}/exists";

        var response = await ExecuteRequest<CheckCollectionExistsResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetCollectionOptimizationProgressResponse> GetCollectionOptimizationProgress(
        string collectionName,
        CancellationToken cancellationToken,
        OptimizationProgressOptionalInfoFields with = OptimizationProgressOptionalInfoFields.None,
        int completedLimit = 16)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteCollection), null);

        var queryParameters = with == OptimizationProgressOptionalInfoFields.None
            ? ""
            : $"?with={GetWithQueryParameter(with)}&completed_limit={completedLimit}";

        var url =
            $"/collections/{collectionName}/optimizations/{queryParameters}";

        var response = await ExecuteRequest<GetCollectionOptimizationProgressResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;

        static string GetWithQueryParameter(OptimizationProgressOptionalInfoFields with)
        {
            const string queued = "queued";
            const string completed = "completed";
            const string idleSegments = "idle_segments";

            if (with.HasFlag(OptimizationProgressOptionalInfoFields.All))
            {
                return $"{queued},{completed},{idleSegments}";
            }

            // 3 possible values
            List<string> selectedWithFields = new(3);

            if (with.HasFlag(OptimizationProgressOptionalInfoFields.Queued))
            {
                selectedWithFields.Add(queued);
            }

            if (with.HasFlag(OptimizationProgressOptionalInfoFields.Completed))

            {
                selectedWithFields.Add(completed);
            }

            if (with.HasFlag(OptimizationProgressOptionalInfoFields.IdleSegments))
            {
                selectedWithFields.Add(idleSegments);
            }

            if (selectedWithFields.Count == 0)
            {
                return "";
            }
            else
            {
                return $"{string.Join(",", selectedWithFields)}";
            }
        }
    }
}
