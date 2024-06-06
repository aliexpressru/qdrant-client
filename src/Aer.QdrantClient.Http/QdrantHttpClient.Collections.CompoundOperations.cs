using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    public async Task<Dictionary<string, GetCollectionInfoResponse.CollectionInfo>> ListCollectionInfo(
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken)
    {
        var listCollectionsResponse = (await ListCollections(cancellationToken)).EnsureSuccess();

        Dictionary<string, GetCollectionInfoResponse.CollectionInfo> ret = new(listCollectionsResponse.Collections.Length);

        foreach (var collectionNameInfo in listCollectionsResponse.Collections)
        {
            var getCollectionInfoResponse = await GetCollectionInfo(
                collectionNameInfo.Name,
                isCountExactPointsNumber,
                cancellationToken);

            ret.Add(collectionNameInfo.Name, getCollectionInfoResponse);
        }

        return ret;
    }
}
