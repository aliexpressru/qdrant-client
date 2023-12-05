namespace Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable UnusedMember.Global

/// <summary>
/// The type of the preset consistency.
/// </summary>
public enum ConsistencyType
{
    /// <summary>
    /// Send N/2+1 random requests and return points, which present on all of them.
    /// </summary>
    Majority,

    /// <summary>
    /// Send requests to all nodes and return points which present on majority of them.
    /// </summary>
    Quorum,

    /// <summary>
    /// Send requests to all nodes and return points which present on all of them.
    /// </summary>
    All
}
