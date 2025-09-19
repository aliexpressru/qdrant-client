using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

#if  NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    private async Task<DefaultOperationResponse> RecoverFromUploadedSnapshot(
        string url,
        Stream snapshotContent,
        CancellationToken cancellationToken)
    {
        var httpMethod = HttpMethod.Post;

        var response = await ExecuteRequestCore<DefaultOperationResponse>(
            CreateMessage,
            cancellationToken,
            retryCount: 0U);

        return response;

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage message = new(httpMethod, url);

            var requestData = new MultipartFormDataContent();
            requestData.Add(new StreamContent(snapshotContent), name: "snapshot");

            message.Content = requestData;

            return message;
        }
    }

    private async Task<DownloadSnapshotResponse> DownloadSnapshot(
        string snapshotName,
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await ExecuteRequestReadAsStream(message, cancellationToken);

            sw.Stop();

            if (!result.IsSuccess)
            {
                // Means the request did not succeed but its processing didn't trigger an exception
                return new DownloadSnapshotResponse(
                    snapshotName,
                    null,
                    -1,
                    new QdrantStatus(QdrantOperationStatusType.Error)
                    {
                        Error = result.ErrorMessage
                    },
                    sw.Elapsed
                );
            }

            return new DownloadSnapshotResponse(
                snapshotName: snapshotName,
                snapshotDataStream: result.ResponseStream,
                snapshotSizeBytes: result.ContentLength,
                new QdrantStatus(QdrantOperationStatusType.Ok),
                sw.Elapsed
            );
        }
        catch (QdrantCommunicationException qce)
        {
            sw.Stop();

            return new DownloadSnapshotResponse(
                snapshotName: snapshotName,
                snapshotDataStream: null,
                snapshotSizeBytes: -1,
                new QdrantStatus(QdrantOperationStatusType.Error)
                {
                    Exception = qce,
                    Error = qce.Message
                },
                sw.Elapsed
            );
        }
        catch (QdrantUnauthorizedAccessException que)
        {
            sw.Stop();

            return new DownloadSnapshotResponse(
                snapshotName,
                snapshotDataStream: null,
                snapshotSizeBytes: -1,
                new QdrantStatus(QdrantOperationStatusType.Error)
                {
                    Exception = que,
                    Error = que.Message
                },
                sw.Elapsed
            );
        }
    }
}
