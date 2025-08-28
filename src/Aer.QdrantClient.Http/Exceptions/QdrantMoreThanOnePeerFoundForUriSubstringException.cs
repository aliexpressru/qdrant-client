using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when more than one peer is found for uri substring.
/// </summary>
/// <param name="peerUriSubstring">The peer uri substring.</param>
/// <param name="foundPeers">The found peers.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantMoreThanOnePeerFoundForUriSubstringException(
	string peerUriSubstring,
	IEnumerable<KeyValuePair<string, ulong>> foundPeers)
	: Exception(
		$"More than one peer found for node uri substring {peerUriSubstring} : [{foundPeers.Select(p => $"Uri = {p.Key}, PeerId = {p.Value}")}]");
