using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Diagnostics.Tracing;
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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(CreateCollection),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(CreateCollection), null);

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureQdrantNameCorrect(collectionName, tracingScope);

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

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    [Obsolete($"This method does not strictly follow the qdrant API shape. Use the {nameof(UpdateCollectionParametersRequest)} overload with the {nameof(CollectionParametersDiffRequest)} parameter. This overload will be removed in the future.")]
    public async Task<DefaultOperationResponse> UpdateCollectionParameters(
        string collectionName,
        UpdateCollectionParametersRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(UpdateCollectionParameters),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(UpdateCollectionParameters), null);

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureQdrantNameCorrect(collectionName, tracingScope);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        if (request.IsEmpty)
        {
            // Empty request is swapped with trigger optimizers request.
            var triggerResult = await TriggerOptimizers(
                collectionName,
                cancellationToken,
                timeout,
                retryCount,
                retryDelay,
                onRetry);

            tracingScope.SetResult(triggerResult);

            return triggerResult;
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

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> UpdateCollectionParameters(
        string collectionName,
        CollectionParametersDiffRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(UpdateCollectionParameters),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(UpdateCollectionParameters), null);

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        EnsureQdrantNameCorrect(collectionName, tracingScope);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}?timeout={timeoutValue}";

        var response = await ExecuteRequest<CollectionParametersDiffRequest, DefaultOperationResponse>(
            url,
            _patchHttpMethod,
            request,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(TriggerOptimizers),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(TriggerOptimizers), null);

        EnsureQdrantNameCorrect(collectionName, tracingScope);

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

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultAsyncOperationResponse> AddDenseNamedVector(
        string collectionName,
        string vectorName,
        ulong vectorSize,
        VectorDistanceMetric vectorDistanceMetric,
        CancellationToken cancellationToken,
        VectorDataType vectorDataType = VectorDataType.Float32,
        MultivectorConfiguration multivectorConfiguration = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(AddDenseNamedVector),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(AddDenseNamedVector), null);

        EnsureQdrantNameCorrect(collectionName, tracingScope);
        EnsureQdrantNameCorrect(vectorName, tracingScope);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}/vectors/{vectorName}?timeout={timeoutValue}";

        var request = new AddNamedVectorRequest(new AddNamedVectorRequest.NewDesnseVectorConfiguration()
        {
            Distance = vectorDistanceMetric.ToString(),
            Size = vectorSize,
            Datatype = vectorDataType,
            MultivectorConfig = multivectorConfiguration
        });

        var response = await ExecuteRequest<AddNamedVectorRequest, DefaultAsyncOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultAsyncOperationResponse> AddSparseNamedVector(
        string collectionName,
        string vectorName,
        CancellationToken cancellationToken,
        SparseVectorModifier modifier = SparseVectorModifier.None,
        VectorDataType vectorDataType = VectorDataType.Float32,
        MultivectorConfiguration multivectorConfiguration = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(AddDenseNamedVector),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(AddDenseNamedVector), null);

        EnsureQdrantNameCorrect(collectionName, tracingScope);
        EnsureQdrantNameCorrect(vectorName, tracingScope);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}/vectors/{vectorName}?timeout={timeoutValue}";

        var request = new AddNamedVectorRequest(new AddNamedVectorRequest.NewSparseVectorConfiguration()
        {
            Modifier = modifier,
            Datatype = vectorDataType
        });

        var response = await ExecuteRequest<AddNamedVectorRequest, DefaultAsyncOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultAsyncOperationResponse> DeleteNamedVector(
        string collectionName,
        string vectorName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeleteNamedVector),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteNamedVector), null);

        EnsureQdrantNameCorrect(collectionName, tracingScope);
        EnsureQdrantNameCorrect(vectorName, tracingScope);

        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}/vectors/{vectorName}?timeout={timeoutValue}";

        var response = await ExecuteRequest<DefaultAsyncOperationResponse>(
            url,
            HttpMethod.Delete,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetCollectionInfo),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetCollectionInfo), clusterName);

        EnsureQdrantNameCorrect(collectionName, tracingScope);

        var url = $"/collections/{collectionName}";

        var response = await ExecuteRequest<GetCollectionInfoResponse>(
            url,
            HttpMethod.Get,
            clusterName ?? collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionsResponse> ListCollections(CancellationToken cancellationToken, string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ListCollections),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ListCollections), clusterName);

        var url = "/collections";

        var response = await ExecuteRequest<ListCollectionsResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeleteCollection),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteCollection), null);

        EnsureQdrantNameCorrect(collectionName, tracingScope);

        var url = $"/collections/{collectionName}?timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ListAllAliases),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ListAllAliases), clusterName);

        var url = "/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ListCollectionAliases),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(ListCollectionAliases), null);

        var url = $"/collections/{collectionName}/aliases";

        var response = await ExecuteRequest<ListCollectionAliasesResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(UpdateCollectionsAliases),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(UpdateCollectionsAliases), clusterName);

        if (updateCollectionAliasesRequest.OperationsCount == 0)
        {
            var ex = new QdrantEmptyBatchRequestException(
                "N/A",
                nameof(UpdateCollectionsAliases),
                updateCollectionAliasesRequest.GetType());

            tracingScope.SetError(ex);

            throw ex;
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

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(CheckCollectionExists),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(CheckCollectionExists), null);

        var url = $"/collections/{collectionName}/exists";

        var response = await ExecuteRequest<CheckCollectionExistsResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CollectionMemoryReportResponse> GetCollectionMemoryReport(
        string collectionName,
        CancellationToken cancellationToken)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetCollectionMemoryReport),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetCollectionMemoryReport), null);

        var url = $"/collections/{collectionName}/memory";

        var response = await ExecuteRequest<CollectionMemoryReportResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetCollectionOptimizationProgress),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetCollectionOptimizationProgress), null);

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

        tracingScope.SetResult(response);

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
