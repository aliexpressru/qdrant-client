namespace Aer.QdrantClient.Http.Infrastructure.Replication;

/// <summary>
/// This is an exception that gets thrown when <see cref="ShardReplicator"/> planning algorithm fails some inbuilt sanity checks.
/// </summary>
public class ShardReplicatorAlgorithmException(string message) : Exception($"Invalid shard replicator algorithm state. {message}");
