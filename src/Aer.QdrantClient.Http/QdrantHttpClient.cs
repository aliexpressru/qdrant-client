using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Exceptions;
    
#if NETSTANDARD2_0 || NETSTANDARD2_1 
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Infrastructure.Tracing;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;

namespace Aer.QdrantClient.Http;

/// <summary>
/// Client for Qdrant HTTP API.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient : IQdrantHttpClient
{
    private readonly ILogger _logger;

    private const int DEFAULT_OPERATION_TIMEOUT_SECONDS = 30;
    private const uint DEFAULT_RETRY_COUNT = 3;

    private static readonly TimeSpan _defaultPointsReadRetryDelay = TimeSpan.FromMilliseconds(100);

    private readonly TimeSpan _defaultOperationTimeout = TimeSpan.FromSeconds(DEFAULT_OPERATION_TIMEOUT_SECONDS);
    private readonly TimeSpan _defaultPollingInterval = TimeSpan.FromSeconds(1);

    // Forbidden status code was issued until qdrant 1.9
    // from 1.9 Unauthorized is issued
    private static readonly HashSet<HttpStatusCode> _unauthorizedStatusCodes =
    [
        HttpStatusCode.Forbidden,
        HttpStatusCode.Unauthorized
    ];
    
    // Codes which should be handled specially
    private static readonly HashSet<HttpStatusCode> _specialStatusCodes =
    [
        // BadRequest, NotFound, Conflict contain error message in their "status" json field,
        // so we need to parse responses with those codes
        HttpStatusCode.BadRequest,
        HttpStatusCode.NotFound,
        HttpStatusCode.Conflict,
        
        .._unauthorizedStatusCodes,
    ];

    // Codes messages with which should not be retried
    private static readonly HashSet<HttpStatusCode> _noRetryStatusCodes =
    [
        HttpStatusCode.NotFound,
        .._unauthorizedStatusCodes
    ];

    private static readonly List<string> _invalidQdrantNameSymbols =
    [
        "/",
        " "
    ];

    internal const string ApiKeyHeaderName = "api-key";

    /// <summary>
    /// The actual HTTP client used to make calls to Qdrant API.
    /// </summary>
    /// <remarks>Protected internal for testing purposes.</remarks>
    protected internal HttpClient ApiClient;

    /// <summary>
    /// Initializes a new Qdrant HTTP client instance.
    /// </summary>
    /// <param name="apiClient">The http client to use.</param>
    /// <param name="logger">The optional logger to log internal messages.</param>
    public QdrantHttpClient(HttpClient apiClient, ILogger logger = null)
    {
        ApiClient = apiClient;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Initializes a new Qdrant HTTP client instance.
    /// </summary>
    /// <param name="host">The qdrant HTTP API host.</param>
    /// <param name="port">The qdrant HTTP API port.</param>
    /// <param name="useHttps">Set to <c>true</c> to use communication over HTTPS, defaults to <c>false</c>.</param>
    /// <param name="apiKey">The Qdrant api key value.</param>
    /// <param name="httpClientTimeout">Http client timeout. Default value is <c>100 seconds</c>.</param>
    /// <param name="logger">The optional logger to log internal messages.</param>
    /// <param name="disableTracing">If set to <c>true</c>, http client activity tracing is disabled.</param>
    public QdrantHttpClient(
        string host,
        int port = 6334,
        bool useHttps = false,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false) : this(
        new UriBuilder(
            useHttps
                ? "https"
                : "http",
            host,
            port).Uri,
        apiKey,
        httpClientTimeout,
        logger,
        disableTracing)
    { }

    /// <summary>
    /// Initializes a new Qdrant HTTP client instance.
    /// </summary>
    /// <param name="httpAddress">The Qdrant HTTP api server address and port.</param>
    /// <param name="apiKey">The Qdrant api key value.</param>
    /// <param name="httpClientTimeout">Http client timeout. Default value is <c>100 seconds</c>.</param>
    /// <param name="logger">The optional logger to log internal messages.</param>
    /// <param name="disableTracing">If set to <c>true</c>, http client activity tracing is disabled.</param>
    public QdrantHttpClient(
        Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false)
    {
        HttpClient apiClient;

        if (disableTracing)
        {
            // Use custom http client handler that disables activity tracing

            DistributedContextPropagator.Current = new ConditionalPropagator();

            apiClient = new HttpClient(new DisableActivityHandler(new HttpClientHandler()))
            {
                BaseAddress = httpAddress,
                Timeout = httpClientTimeout ?? QdrantClientSettings.DefaultHttpClientTimeout,
            };
        }
        else
        {
            apiClient = new HttpClient()
            {
                BaseAddress = httpAddress,
                Timeout = httpClientTimeout ?? QdrantClientSettings.DefaultHttpClientTimeout
            };
        }

        if (apiKey is {Length: > 0})
        {
            apiClient.DefaultRequestHeaders.Add(
                ApiKeyHeaderName,
                apiKey
            );
        }

        ApiClient = apiClient;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public async Task EnsureCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? pollingInterval = null,
        TimeSpan? timeout = null,
        uint requiredNumberOfGreenCollectionResponses = 1,
        bool isCheckShardTransfersCompleted = false)
    {
        if (timeout is {TotalMilliseconds: 0})
        {
            throw new InvalidOperationException(
                $"{nameof(timeout)} should be greater than zero or not set but was {timeout:g}");
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

            if (isCheckShardTransfersCompleted)
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

    private async Task<TResponse> ExecuteRequest<TResponse>(
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        if (message.RequestUri is null)
        {
            throw new InvalidOperationException("Message request uri is null");
        }

        var response = await ApiClient.SendAsync(message, cancellationToken);

        var result = await ReadResponseAndHandleErrors(
            message,
            response,
            responseReader: async responseMessage =>
            {
                var resultString =
                    await responseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (typeof(TResponse) == typeof(string))
                {
                    // If the result type is string - return result as is
                    return (TResponse) Convert.ChangeType(resultString, typeof(TResponse));
                }

                var deserializedResult =
                    JsonSerializer.Deserialize<TResponse>(
                        resultString,
                        JsonSerializerConstants.DefaultSerializerOptions);

                return deserializedResult;
            },
            badRequestResponseMessageReader: null,
            cancellationToken);

        return result;
    }

    private Task<TResponse> ExecuteRequest<TResponse>(
        string url,
        HttpMethod method,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
        where TResponse : QdrantResponseBase
        =>
            ExecuteRequestCore<TResponse>(
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
        Action<Exception, TimeSpan, int, uint> onRetry = null)
        where TRequest : class
        where TResponse : QdrantResponseBase
    {
        var response = ExecuteRequestCore<TResponse>(
            CreateMessage,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage message = new(method, url);

            var contentJson = requestContent switch
            {
                EmptyRequest er => er.RequestMessageBody,

                // check whether the requestContent is already a serialized string
                string s => s,

                not null => JsonSerializer.Serialize(requestContent, JsonSerializerConstants.DefaultSerializerOptions),
                null => EmptyRequest.Instance.RequestMessageBody
            };

            var requestData = new StringContent(contentJson, Encoding.UTF8, "application/json");

            message.Content = requestData;

            return message;
        }
    }

    private async Task<(long ContentLength, Stream ResponseStream, bool IsSuccess, string ErrorMessage)> ExecuteRequestReadAsStream(
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        if (message.RequestUri is null)
        {
            throw new InvalidOperationException("Message request uri is null");
        }

        var response = await ApiClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var result = await ReadResponseAndHandleErrors(
            message,
            response,
            responseReader: async responseMessage =>
            {
                // Handle NotFound for read as stream specially since it contains error message in content and no Status field
                if(responseMessage.StatusCode == HttpStatusCode.NotFound)
                { 
                    var errorMessage = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

                    return (
                        ContentLength: 0L,
                        ResponseStream: Stream.Null,
                        IsSuccess: false,
                        ErrorMessage: errorMessage
                    );
                }
                
                var resultStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

                var contentLength = response.Content.Headers.ContentLength ?? 0;

                return (contentLength, resultStream, true, null);
            },
            badRequestResponseMessageReader: null,
            cancellationToken);

        return result;
    }

    private async Task<TResponse> ExecuteRequestCore<TResponse>(
        // We are using func to create message since sending one instance of HttpRequestMessage several times is not allowed.
        Func<HttpRequestMessage> createMessage,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
        where TResponse : QdrantResponseBase
    {
        var getResponse =
            async () => await ApiClient.SendAsync(createMessage(), cancellationToken);

        if (retryCount > 0)
        {
            getResponse = () => Policy
                .Handle<HttpRequestException>(
                    // Retry only responses without status code or when status code is not a special one

#if NETSTANDARD2_0 || NETSTANDARD2_1
                    e => e.GetStatusCode() is null
                        ||
                        (e.GetStatusCode() is { } statusCode && !_specialStatusCodes.Contains(statusCode))
#else
                    e => e.StatusCode is null
                        ||
                        (e.StatusCode is { } statusCode && !_specialStatusCodes.Contains(statusCode))
#endif

                )
                // Also retry when response status code is not successful and not a special no-retry one
                .OrResult<HttpResponseMessage>(r =>
                    !r.IsSuccessStatusCode && !_noRetryStatusCodes.Contains(r.StatusCode))
                .WaitAndRetryAsync(
                    (int) retryCount,
                    _ => retryDelay ?? _defaultPointsReadRetryDelay,
                    onRetry: (result, currentRetryDelay, retryNumber, _) =>
                    {
                        // result.Exception can be null when retrying not the exceptional case but unsuccessful status code
                        // To avoid null reference exception we substitute null exceptions with a special QdrantRequestRetryException
                        onRetry?.Invoke(
                            result.Exception ?? new QdrantRequestRetryException(result.Result),
                            currentRetryDelay,
                            retryNumber,
                            retryCount);
                    }
                )
                .ExecuteAsync(
#if NETSTANDARD2_0
                    async () => (await ApiClient.SendAsync(createMessage(), cancellationToken)).SetStatusCode()
#else
                    () => ApiClient.SendAsync(createMessage(), cancellationToken)
#endif
                );
        }

        var responseMessage = await getResponse();

        // createMessage() should never be called unless there is some non-standard http message handler in HttpClient that does not set the RequestMessage property.
        // See https://github.com/dotnet/runtime/discussions/104113 for details.
        var requestMessage = responseMessage.RequestMessage ?? createMessage();

        var result = await ReadResponseAndHandleErrors(
            requestMessage,
            responseMessage,
            responseReader: async rm =>
            {
                // Handle NotFound with empty content. This happens when making calls to non-existent Qdrant API endpoints
                if (responseMessage.StatusCode == HttpStatusCode.NotFound
                    && responseMessage.Content.Headers.ContentLength == 0)
                {
                    throw new QdrantCommunicationException(
                        requestMessage.Method.ToString(),
                        requestMessage.RequestUri?.ToString(),
                        responseMessage.StatusCode,
                        responseMessage.ReasonPhrase,
                        string.Empty);
                }

                var resultString = await rm.Content.ReadAsStringAsync(cancellationToken);

                var deserializedObject =
                    JsonSerializer.Deserialize<TResponse>(
                        resultString,
                        JsonSerializerConstants.DefaultSerializerOptions);

                return deserializedObject;
            },
            badRequestResponseMessageReader: errorContent =>
            {
                // in case of bad request the result may be in the form of a single string
                // thus the following parsing may fail
                try
                {
                    var badRequestResult =
                        JsonSerializer.Deserialize<TResponse>(
                            errorContent,
                            JsonSerializerConstants.DefaultSerializerOptions);

                    return badRequestResult;
                }
                catch (Exception ex)
                {
                    // means that the response is a simple string or some other unexpected error happened
                    var errorResponse = Activator.CreateInstance<TResponse>();

                    errorResponse.Status = new QdrantStatus(QdrantOperationStatusType.Unknown)
                    {
                        Error = errorContent,
                        RawStatusString = errorContent,
                        Exception = ex is JsonException
                            ? null
                            : ex // do not set exception if it is a json exception
                    };

                    errorResponse.Time = -1;

                    return errorResponse;
                }
            },
            cancellationToken);

        return result;
    }

    private async Task<TResponse> ReadResponseAndHandleErrors<TResponse>(
        HttpRequestMessage requestMessage,
        HttpResponseMessage responseMessage,
        Func<HttpResponseMessage, Task<TResponse>> responseReader,
        Func<string, TResponse> badRequestResponseMessageReader,
        CancellationToken cancellationToken)
    {
        // We have already checked that the request uri is not null
        var url = requestMessage.RequestUri!.ToString();
        
        // Throw if the response status code is not successful and not a special one
        if (!responseMessage.IsSuccessStatusCode
            && !_specialStatusCodes.Contains(responseMessage.StatusCode))
        {
            var errorResultString =
                await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            throw new QdrantCommunicationException(
                requestMessage.Method.Method,
                url,
                responseMessage.StatusCode,
                responseMessage.ReasonPhrase,
                errorResultString);
        }
        
        // Handle unauthorized codes
        if (_unauthorizedStatusCodes.Contains(responseMessage.StatusCode))
        {
            var forbiddenReason = responseMessage.ReasonPhrase;

            try
            {
                // Try to read specific error from content
                var errorMessage = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    forbiddenReason = $"{responseMessage.ReasonPhrase} : {errorMessage}";
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            throw new QdrantUnauthorizedAccessException(forbiddenReason);
        }

        // Handle bad request
        if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorResult = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (badRequestResponseMessageReader == null)
            {
                throw new QdrantCommunicationException(
                    requestMessage.Method.Method,
                    url,
                    responseMessage.StatusCode,
                    responseMessage.ReasonPhrase,
                    errorResult);
            }

            return badRequestResponseMessageReader(errorResult);
        }
        
        var readResult = await responseReader(responseMessage);

        return readResult;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureQdrantNameCorrect(string qdrantEntityName)
    {
        if (qdrantEntityName is null or {Length: 0})
        {
            throw new QdrantInvalidEntityNameException(
                qdrantEntityName,
                "Entity name name should not be null or empty");
        }

        if (qdrantEntityName.Length is 0 or > 255)
        {
            throw new QdrantInvalidEntityNameException(
                qdrantEntityName,
                $"Entity name should be between 1 and 255 characters long. Length of {qdrantEntityName.Length} is found.");
        }

        if (_invalidQdrantNameSymbols.Any(qdrantEntityName.Contains))
        {
            throw new QdrantInvalidEntityNameException(
                qdrantEntityName,
                $"Entity name can't contain [{string.Join(",", _invalidQdrantNameSymbols)}] symbols");
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
