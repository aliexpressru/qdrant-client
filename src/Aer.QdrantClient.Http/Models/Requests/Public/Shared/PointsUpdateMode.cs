namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Mode of the points upsert operation.
/// </summary>
public enum PointsUpdateMode
{
    /// <summary>
    /// Insert a point if it does not exist, or update it if it does.
    /// </summary>
    Upsert,

    /// <summary>
    /// Insert a point only if it does not already exist.
    /// If a point with the same ID exists, the operation is ignored.
    /// </summary>
    InsertOnly,

    /// <summary>
    /// Update a point only if it already exists. Points that do not exist are not inserted.
    /// </summary>
    UpdateOnly
}
