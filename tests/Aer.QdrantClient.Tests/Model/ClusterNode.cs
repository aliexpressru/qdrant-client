namespace Aer.QdrantClient.Tests.Model;

public enum ClusterNode
{
    First,

    Second,

    // Third node exists only for 3-node cluster (duh).
    // Since we are having two multi-node clusters - 2 nodes and 3 nodes -
    // we use third node to distinguish between the two.
    // So if asking for third node - we are also asking for a 3 node cluster
    Third
}
