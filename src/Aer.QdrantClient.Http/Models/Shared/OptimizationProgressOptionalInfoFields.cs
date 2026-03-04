namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the selected list of optional optimisation progress fields
/// </summary>
[Flags]
public enum OptimizationProgressOptionalInfoFields
{
    /// <summary>
    /// No optional fields selected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Return a list of queued optimisations.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// return a list of completed optimisations.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Return a list of idle segments.
    /// </summary>
    IdleSegments = 4,

    /// <summary>
    /// Get all optional optimisation data fields.
    /// </summary>
    All = 7
}
