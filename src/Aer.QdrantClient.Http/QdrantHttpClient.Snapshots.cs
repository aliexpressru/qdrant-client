using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Diagnostics.Helpers;
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
        string collectionOrClusterName,
        Stream snapshotContent,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionOrClusterName, nameof(RecoverFromUploadedSnapshot), null);

        var httpMethod = HttpMethod.Post;

        var response = await ExecuteRequestCore<DefaultOperationResponse>(
            CreateMessage,
            collectionOrClusterName,
            cancellationToken,
            retryCount: 0U);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        string collectionOrClusterName,
        string snapshotName,
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionOrClusterName, nameof(DownloadSnapshot), null);

        var sw = Stopwatch.StartNew();

        try
        {
            var (contentLength, responseStream, isSuccess, errorMessage)
                = await ExecuteRequestReadAsStream(message, collectionOrClusterName, cancellationToken);

            sw.Stop();

            if (!isSuccess)
            {
                // Means the request did not succeed but its processing didn't trigger an exception
                return new DownloadSnapshotResponse(
                    snapshotName,
                    null,
                    -1,
                    new QdrantStatus(QdrantOperationStatusType.Error)
                    {
                        Error = errorMessage
                    },
                    sw.Elapsed
                );
            }

            diagnostic.SetSuccess();

            return new DownloadSnapshotResponse(
                snapshotName: snapshotName,
                snapshotDataStream: responseStream,
                snapshotSizeBytes: contentLength,
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
