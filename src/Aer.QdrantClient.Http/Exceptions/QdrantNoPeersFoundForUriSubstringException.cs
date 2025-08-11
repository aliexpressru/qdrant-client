using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when no peers found for uri substring.
/// </summary>
/// <param name="peerUriSubstring">The peer uri substring.</param>
/// <param name="existingPeers">The existing peers.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantNoPeersFoundForUriSubstringException(
	string peerUriSubstring,
	IEnumerable<KeyValuePair<string, ulong>> existingPeers)
	: Exception(
		$"No peers found for node uri substring {peerUriSubstring} existing peers : [{existingPeers.Select(p => $"Uri = {p.Key}, PeerId = {p.Value}")}]");
