namespace Aer.QdrantClient.Http.Exceptions;

internal class QdrantNoNodesFoundForUriSubstringException : Exception
{
    public QdrantNoNodesFoundForUriSubstringException(
        string clusterNodeUriSubstring,
        IEnumerable<KeyValuePair<string, ulong>> existingPeers) : base(
        $"No peers found for node uri substring {clusterNodeUriSubstring} existing peers : [{existingPeers.Select(p => $"Uri = {p.Key}, PeerId = {p.Value}")}]")
    { }
}
