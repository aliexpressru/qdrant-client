namespace Aer.QdrantClient.Http.Exceptions;

internal class QdrantMoreThanOneNodeFoundForUriSubstringException : Exception
{
    public QdrantMoreThanOneNodeFoundForUriSubstringException(
        string clusterNodeUriSubstring,
        IEnumerable<KeyValuePair<string, ulong>> foundPeers) : base(
        $"More than one peer found for node uri substring {clusterNodeUriSubstring} : [{foundPeers.Select(p => $"Uri = {p.Key}, PeerId = {p.Value}")}]")
    { }
}
