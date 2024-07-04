using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when more no peers found for uri substring.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantNoPeersFoundForUriSubstringException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="QdrantMoreThanOnePeerFoundForUriSubstringException"/>.
    /// </summary>
    /// <param name="peerUriSubstring">The peer uri substring.</param>
    /// <param name="existingPeers">The existing peers.</param>
    public QdrantNoPeersFoundForUriSubstringException(
        string peerUriSubstring,
        IEnumerable<KeyValuePair<string, ulong>> existingPeers) : base(
        $"No peers found for node uri substring {peerUriSubstring} existing peers : [{existingPeers.Select(p => $"Uri = {p.Key}, PeerId = {p.Value}")}]")
    { }
}
