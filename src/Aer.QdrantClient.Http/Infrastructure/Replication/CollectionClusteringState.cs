using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Infrastructure.Replication;

#pragma warning disable CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method

/// <summary>
/// Represents a collection clustering state, either current, in the past or in the future.
/// </summary>
internal class CollectionClusteringState
{
    private int _version;

    /// <summary>
    /// Gets the mapping of peer identifiers to the list of shard identifiers replicated on them .
    /// </summary>
    public Dictionary<ulong, HashSet<uint>> ShardsByPeers { get; internal set; }

    /// <summary>
    /// Gets the mapping of shard identifiers to the list of peer identifiers they are replicated on.
    /// </summary>
    public Dictionary<uint, HashSet<ulong>> PeersByShards { get; internal set; }

    /// <summary>
    /// Gets the mapping from the peer id to peer info which contains the peer uri.
    /// </summary>
    public Dictionary<ulong, Peer> KnownPeers { get; internal set; }

    /// <summary>
    /// Number of shards a collection has.
    /// </summary>
    public int ShardCount { get; internal set; }

    public CollectionClusteringState(
        GetClusterInfoResponse.ClusterInfo clusterInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
    )
    {
        Dictionary<ulong, Peer> knownPeers = new(clusterInfo.ParsedPeers.Count);

        foreach (var peer in clusterInfo.ParsedPeers)
        {
            var peerId = peer.Key;
            var peerUri = peer.Value.Uri;

            knownPeers.Add(peerId, new(peerId, peerUri));
        }

        KnownPeers = knownPeers;

        ShardsByPeers = [];
        PeersByShards = [];

        HashSet<uint> allShardIds = [];

        foreach (var shardsByPeer in collectionClusteringInfo.ShardsByPeers)
        {
            ulong peerId = shardsByPeer.Key;
            var shards = shardsByPeer.Value;

            ShardsByPeers.Add(peerId, [.. shards]);
        }

        foreach (var peersByShard in collectionClusteringInfo.PeersByShards)
        {
            uint shardId = peersByShard.Key;
            var peers = peersByShard.Value;

            PeersByShards.Add(shardId, [.. peers]);

            allShardIds.Add(shardId);
        }

        ShardCount = allShardIds.Count;
    }

    public bool AddShardReplica(uint shardId, ulong newPeerId)
    {
        bool wasStateModified = false;

        if (!PeersByShards.ContainsKey(shardId))
        {
            PeersByShards[shardId] = [];
        }

        wasStateModified |= PeersByShards[shardId].Add(newPeerId);

        if (!ShardsByPeers.ContainsKey(newPeerId))
        {
            ShardsByPeers[newPeerId] = [];
        }

        wasStateModified |= ShardsByPeers[newPeerId].Add(shardId);

        _version++;

        return wasStateModified;
    }

    public bool DropShardReplica(uint shardId, ulong removeFromPeerId)
    {
        bool wasStateModified = false;

        if (PeersByShards.ContainsKey(shardId))
        {
            wasStateModified |= PeersByShards[shardId].Remove(removeFromPeerId);
        }

        if (ShardsByPeers.ContainsKey(removeFromPeerId))
        {
            wasStateModified |= ShardsByPeers[removeFromPeerId].Remove(shardId);
        }

        if (wasStateModified)
        {
            _version++;
        }

        return wasStateModified;
    }

    public bool MoveShardReplica(uint shardId, ulong sourcePeerId, ulong targetPeerId)
    {
        bool wasStateModified = false;

        wasStateModified |= DropShardReplica(shardId, sourcePeerId);
        wasStateModified |= AddShardReplica(shardId, targetPeerId);

        if (wasStateModified)
        {
            _version++;
        }

        return wasStateModified;
    }

    /// <summary>
    /// Represents a cluster peer.
    /// </summary>
    /// <param name="Id">The peer id.</param>
    /// <param name="Uri">The peer URI.</param>
    internal record Peer(ulong Id, string Uri);
}

#pragma warning restore CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method
