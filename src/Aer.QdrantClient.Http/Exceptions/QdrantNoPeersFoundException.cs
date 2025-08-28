using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when no peers found for peer id.
/// </summary>
/// <param name="peerId">The peer id.</param>
/// <param name="existingPeers">The existing peers.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantNoPeersFoundException(
    ulong peerId,
    IEnumerable<ulong> existingPeers)
    : Exception(
        $"No peers found for node peer id {peerId} existing peers : [{string.Join(", ", existingPeers.Select(p => p.ToString()))}]");
