using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;
using Polly;

namespace Aer.QdrantClient.Http;

/// <summary>
/// Client for Qdrant HTTP API.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    private readonly HttpClient _apiClient;
    private const int DEFAULT_OPERATION_TIMEOUT_SECONDS = 30;

    private const uint DEFAULT_POINTS_READ_RETRY_COUNT = 3;
    private static readonly TimeSpan _defaultPointsReadRetryDelay = TimeSpan.FromSeconds(1);

    private readonly TimeSpan _defaultOperationTimeout = TimeSpan.FromSeconds(DEFAULT_OPERATION_TIMEOUT_SECONDS);
    private readonly TimeSpan _defaultPollingInterval = TimeSpan.FromSeconds(1);

    // Forbidden status code was issued until qdrant 1.9
    // from 1.9 Unauthorized is issued
    private static readonly HashSet<HttpStatusCode> _unauthorizedStatusCodes =
    [
        HttpStatusCode.Forbidden,
        HttpStatusCode.Unauthorized
    ];

    // contains codes which should be handled specially
    private static readonly HashSet<HttpStatusCode> _specialStatusCodes =
    [
        // BadRequest, NotFound, Conflict contain error message in their "status" json field,
        // so we need to parse responses with those codes
        HttpStatusCode.BadRequest,
        HttpStatusCode.NotFound,
        HttpStatusCode.Conflict,

        // Forbidden and Unauthorized are handled specially to throw unauthorized exception
        .. _unauthorizedStatusCodes
    ];

    private static readonly List<string> _invalidQdrantNameSymbols =
    [
        "/",
        " "
    ];

    /// <summary>
    /// Initializes a new Qdrant HTTP client instance.
    /// </summary>
    /// <param name="apiClient">The http client to use.</param>
    public QdrantHttpClient(HttpClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Initializes a new Qdrant HTTP client instance.
    /// </summary>
    /// <param name="host">The qdrant HTTP API host.</param>
    /// <param name="port">The qdrant HTTP API port.</param>
    /// <param name="useHttps">Set to <c>true</c> to use communication over HTTPS, defaults to <c>false</c>.</param>
    /// <param name="apiKey">The Qdrant api key value.</param>
    /// <param name="httpClientTimeout">Http client timeout. Default value is <c>100 seconds</c>.</param>
    public QdrantHttpClient(
        string host,
        int port = 6334,
        bool useHttps = false,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null) : this(
        new UriBuilder(
            useHttps
                ? "https"
                : "http",
            host,
            port).Uri,
        apiKey,
        httpClientTimeout)
    { }

    /// <summary>
    /// Initializes a new Qdrant HTTP client instance.
    /// </summary>
    /// <param name="httpAddress">The Qdrant HTTP api server address and port.</param>
    /// <param name="apiKey">The Qdrant api key value.</param>
    /// <param name="httpClientTimeout">Http client timeout. Default value is <c>100 seconds</c>.</param>
    public QdrantHttpClient(Uri httpAddress, string apiKey = null, TimeSpan? httpClientTimeout = null)
    {
        var apiClient = new HttpClient()
        {
            BaseAddress = httpAddress,
            Timeout = httpClientTimeout ?? QdrantClientSettings.DefaultHttpClientTimeout
        };

        if (apiKey is {Length: > 0})
        {
            apiClient.DefaultRequestHeaders.Add(
                "api-key",
                apiKey
            );
        }

        _apiClient = apiClient;
    }

    /// <summary>
    /// Asynchronously wait until the collection status becomes <see cref="QdrantCollectionStatus.Green"/>
    /// and collection optimizer status becomes <see cref="QdrantOptimizerStatus.Ok"/>.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check status for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="pollingInterval">The collection status polling interval. Is not set the default polling interval is 1 second.</param>
    /// <param name="timeout">The timeout after which the collection considered not green and exception is thrown. The default timeout is 30 seconds.</param>
    /// <param name="requiredNumberOfGreenCollectionResponses">The number of green status responses to be received
    /// for collection status to be considered green. To increase the probability that every node has
    /// the same green status - set this parameter to a value greater than the number of nodes.</param>
    /// <param name="checkAllCollectionShardTransfersCompleted">
    /// If set to <c>true</c> check that all collection shard transfers are completed.
    /// The collection is not considered ready until all shard transfers are completed.
    /// </param>
    public async Task EnsureCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? pollingInterval = null,
        TimeSpan? timeout = null,
        uint requiredNumberOfGreenCollectionResponses = 1,
        bool checkAllCollectionShardTransfersCompleted = false)
    {
        if (timeout is {TotalMilliseconds: 0})
        {
            throw new InvalidOperationException($"{nameof(timeout)} should be greater than zero or not set but was {timeout:g}");
        }

        var actualTimeout = timeout ?? _defaultOperationTimeout;
        var actualPollingInterval = pollingInterval ?? _defaultPollingInterval;

        if (actualPollingInterval >= actualTimeout)
        {
            throw new InvalidOperationException(
                $"{nameof(timeout)} {actualTimeout:g} should be greater than {nameof(pollingInterval)} {actualPollingInterval:g} interval");
        }

        var pollingEndTime = DateTime.Now.Add(actualTimeout);

        var requiredCollectionIsReadyResponsesLeft = requiredNumberOfGreenCollectionResponses;

        while (requiredCollectionIsReadyResponsesLeft > 0)
        {
            if (DateTime.Now > pollingEndTime)
            {
                throw new QdrantCollectionNotGreenException(collectionName, actualTimeout);
            }

            var collectionInfo = (await GetCollectionInfo(collectionName, cancellationToken)).EnsureSuccess();

            bool isCollectionShardTransfersCompleted = true;

            if (checkAllCollectionShardTransfersCompleted)
            {
                var collectionClusteringInfo =
                    (await GetCollectionClusteringInfo(collectionName, cancellationToken)).EnsureSuccess();

                isCollectionShardTransfersCompleted = collectionClusteringInfo.ShardTransfers.Length == 0;
            }

            if (collectionInfo.Status is QdrantCollectionStatus.Green
                && collectionInfo.OptimizerStatus.IsOk
                && isCollectionShardTransfersCompleted)
            {
                requiredCollectionIsReadyResponsesLeft--;

                if (requiredCollectionIsReadyResponsesLeft == 0)
                {
                    // in this case there is no point in waiting for polling interval to elapse
                    break;
                }
            }

            await Task.Delay(actualPollingInterval, cancellationToken);
        }
    }

    private async Task<string> ExecuteRequestPlain(
        string url,
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.SendAsync(message, cancellationToken);

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new QdrantCommunicationException(
                message.Method.Method,
                url,
                response.StatusCode,
                response.ReasonPhrase,
                result);
        }

        return result;
    }

    private Task<TResponse> ExecuteRequest<TResponse>(
        string url,
        HttpMethod method,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int> onRetry = null)
        where TResponse : QdrantResponseBase
        =>
            ExecuteRequestCore<TResponse>(
                url,
                method,
                () => new(method, url),
                cancellationToken,
                retryCount,
                retryDelay,
                onRetry);

    private Task<TResponse> ExecuteRequest<TRequest, TResponse>(
        string url,
        HttpMethod method,
        TRequest requestContent,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int> onRetry = null)
        where TRequest : class
        where TResponse : QdrantResponseBase
    {
        var response = ExecuteRequestCore<TResponse>(
            url,
            method,
            CreateMessage,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage message = new(method, url);

            var contentJson =
                // check whether the requestContent is already a serialized string
                requestContent as string
                ?? JsonSerializer.Serialize(requestContent, JsonSerializerConstants.SerializerOptions);

            var requestData = new StringContent(contentJson, Encoding.UTF8, "application/json");

            message.Content = requestData;

            return message;
        }
    }

    private async Task<(long ContentLength, Stream ResponseStream)> ExecuteRequestReadAsStream(
        string url,
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        var response = await _apiClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode
            && !_specialStatusCodes.Contains(response.StatusCode))
        {
            var errorResult = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new QdrantCommunicationException(
                message.Method.Method,
                url,
                response.StatusCode,
                response.ReasonPhrase,
                errorResult);
        }

        // handle unauthorized codes
        if (_unauthorizedStatusCodes.Contains(response.StatusCode))
        {
            throw new QdrantUnauthorizedAccessException(response.ReasonPhrase);
        }

        // in case of bad request the result may be in the form of a single string
        // thus the following parsing may fail

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorResult = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new QdrantCommunicationException(
                message.Method.Method,
                url,
                response.StatusCode,
                response.ReasonPhrase,
                errorResult);
        }

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        var resultStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return (contentLength, resultStream);
    }

    private async Task<TResponse> ExecuteRequestCore<TResponse>(
        string url,
        HttpMethod httpMethod,
        // We are using func to create message since sending one instance of HttpRequestMessage several times is not allowed.
        Func<HttpRequestMessage> createMessage,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int> onRetry = null)
        where TResponse : QdrantResponseBase
    {
        var getResponse =
            async () => await _apiClient.SendAsync(createMessage(), cancellationToken);

        if (retryCount > 0)
        {
            getResponse = () => Policy
                .Handle<HttpRequestException>(
                    e => e.StatusCode is null
                        ||
                        (e.StatusCode is { } statusCode && !_specialStatusCodes.Contains(statusCode))
                )
                .WaitAndRetryAsync(
                    (int) retryCount,
                    _ => retryDelay ?? _defaultPointsReadRetryDelay,
                    onRetry: (exception, currentRetryDelay, retryNumber, _) =>
                    {
                        onRetry?.Invoke(exception, currentRetryDelay, retryNumber);
                    }
                )
                .ExecuteAsync(() => _apiClient.SendAsync(createMessage(), cancellationToken));
        }

        var response = await getResponse();

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode
            && !_specialStatusCodes.Contains(response.StatusCode))
        {
            throw new QdrantCommunicationException(
                httpMethod.Method,
                url,
                response.StatusCode,
                response.ReasonPhrase,
                result);
        }

        // handle unauthorized codes
        if (_unauthorizedStatusCodes.Contains(response.StatusCode))
        {
            throw new QdrantUnauthorizedAccessException(result);
        }

        // in case of bad request the result may be in the form of a single string
        // thus the following parsing may fail

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            try
            {
                var badRequestResult =
                    JsonSerializer.Deserialize<TResponse>(result, JsonSerializerConstants.SerializerOptions);

                return badRequestResult;
            }
            catch (JsonException jex)
            {
                // means that the response is a simple string
                var errorResponse = Activator.CreateInstance<TResponse>();
                errorResponse.Status = new QdrantStatus(QdrantOperationStatusType.Unknown)
                {
                    Error = result,
                    RawStatusString = result,
                    Exception = jex
                };
                errorResponse.Time = -1;

                return errorResponse;
            }
        }

        var deserializedObject =
            JsonSerializer.Deserialize<TResponse>(result, JsonSerializerConstants.SerializerOptions);

        return deserializedObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureQdrantNameCorrect(string qdrantEntityName)
    {
        if (qdrantEntityName is null or {Length: 0})
        {
            throw new InvalidOperationException("Qdrant entity name name should not be null or empty");
        }

        if (_invalidQdrantNameSymbols.Any(qdrantEntityName.Contains))
        {
            throw new InvalidOperationException(
                $"Qdrant entity name {qdrantEntityName} can't contain [{string.Join(",", _invalidQdrantNameSymbols)}] symbols");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToUrlQueryString(bool target)
        =>
            target switch
            {
                true => "true",
                false => "false"
            };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToUrlQueryString<TEnum>(TEnum enumValue)
        where TEnum : Enum
    {
        var convertedEnumName =
            JsonSerializerConstants.NamingStrategy.ConvertName(enumValue.ToString());

        return convertedEnumName;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetTimeoutValueOrDefault(TimeSpan? timeout)
        => timeout?.TotalSeconds ?? DEFAULT_OPERATION_TIMEOUT_SECONDS;
}
