using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Infrastructure.Replication;

#pragma warning disable CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method

/// <summary>
/// Represents a collection clustering state, either current, in the past or in the future.
/// </summary>
internal class CollectionClusteringState
{
    /// <summary>
    /// Represents a cluster peer.
    /// </summary>
    /// <param name="Id">The peer id.</param>
    /// <param name="Uri">The peer URI.</param>
    internal record Peer(ulong Id, string Uri);

    //private int _version;
    private readonly int _targetReplicationFactor;

    /// <summary>
    /// Gets the mapping of peer identifiers to the list of shard identifiers replicated on them .
    /// </summary>
    public Dictionary<ulong, HashSet<uint>> ShardsByPeers { get; }

    /// <summary>
    /// Gets the mapping of shard identifiers to the list of peer identifiers they are replicated on.
    /// </summary>
    public Dictionary<uint, HashSet<ulong>> PeersByShards { get; }

    /// <summary>
    /// Gets the mapping from the peer id to peer info which contains the peer uri.
    /// </summary>
    public Dictionary<ulong, Peer> KnownPeers { get; }

    /// <summary>
    /// Number of shards a collection has.
    /// </summary>
    public int ShardCount { get; }

    public int MaxNumberOfReplicasPerPeer { get; }

    public int MinNumberOfReplicasPerPeer { get; }

    /// <summary>
    /// Represents number of times this state was modified from the initial creation.
    /// We use this property as a replication operation number in replication plan.
    /// </summary>
    public int Version { get; private set; } = -1;

    public CollectionClusteringState(
        GetClusterInfoResponse.ClusterInfo clusterInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo,
        int targetReplicationFactor
    )
    {
        // Replace with a line below when collection expression parameters support lands in C#15
        Dictionary<ulong, Peer> knownPeers = [];
        // Dictionary<ulong, Peer> knownPeers = [with(clusterInfo.ParsedPeers.Count)];

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

        _targetReplicationFactor = targetReplicationFactor;

        var expectedNumberOfReplicasPerPeer = (double)targetReplicationFactor * ShardCount / KnownPeers.Count;

        MaxNumberOfReplicasPerPeer = (int)Math.Ceiling(expectedNumberOfReplicasPerPeer);
        MinNumberOfReplicasPerPeer = (int)Math.Floor(expectedNumberOfReplicasPerPeer);
    }

    public bool AddShardReplica(uint shardId, ulong newPeerId, bool shouldUpdateVersion = true)
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

        if (wasStateModified && shouldUpdateVersion)
        {
            Version++;
        }

        return wasStateModified;
    }

    public bool DropShardReplica(uint shardId, ulong removeFromPeerId, bool shouldUpdateVersion = true)
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

        if (wasStateModified && shouldUpdateVersion)
        {
            Version++;
        }

        return wasStateModified;
    }

    public bool MoveShardReplica(uint shardId, ulong sourcePeerId, ulong targetPeerId)
    {
        bool wasStateModified = false;

        // We are passing `shouldUpdateVersion: false` to each of the operations to only update version once

        wasStateModified |= DropShardReplica(shardId, sourcePeerId, shouldUpdateVersion: false);
        wasStateModified |= AddShardReplica(shardId, targetPeerId, shouldUpdateVersion: false);

        if (wasStateModified)
        {
            Version++;
        }

        return wasStateModified;
    }

    /// <summary>
    /// Gets the id of the peer with minimum number of replicas.
    /// </summary>
    public (ulong PeerId, HashSet<uint> ShardIds) GetMinReplicasPeer()
    {
        var minReplicasPeer = ShardsByPeers.MinBy(p => p.Value.Count);

        return (minReplicasPeer.Key, minReplicasPeer.Value);
    }

    /// <summary>
    /// Gets the id of the peer with maximum number of replicas.
    /// </summary>
    public (ulong PeerId, HashSet<uint> ShardIds) GetMaxReplicasPeer()
    {
        var maxReplicasPeer = ShardsByPeers.MaxBy(p => p.Value.Count);

        return (maxReplicasPeer.Key, maxReplicasPeer.Value);
    }

    /// <summary>
    /// Gets the peers with number of replicas larger than maximum expected.
    /// </summary>
    public (ulong PeerId, HashSet<uint> ShardIds)? GetMostOverpopulatedPeer()
    {
        var overpopulatedPeer = ShardsByPeers
                .Where(peerWithShards => peerWithShards.Value.Count > MaxNumberOfReplicasPerPeer)
                .OrderByDescending(p => p.Value.Count)
                .FirstOrDefault();

        if (overpopulatedPeer.Key == 0 && overpopulatedPeer.Value == null)
        {
            // Means no overpopulated peers left
            return null;
        }

        return (overpopulatedPeer.Key, overpopulatedPeer.Value);
    }

    public (ulong PeerId, HashSet<uint> ShardIds)? GetMostUnderpopulatedPeer()
    {
        var underpopulatedPeer = ShardsByPeers
                .Where(peerWithShards => peerWithShards.Value.Count < MinNumberOfReplicasPerPeer)
                .OrderBy(p => p.Value.Count)
                .FirstOrDefault();

        if (underpopulatedPeer.Key == 0 && underpopulatedPeer.Value == null)
        {
            // Means no underpopulated peers left
            return null;
        }

        return (underpopulatedPeer.Key, underpopulatedPeer.Value);
    }
}

#pragma warning restore CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method
