// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a returned point vector selector. Used to select named vectors in return result.
/// </summary>
public abstract class VectorSelector
{
    internal class AllVectorsSelector : VectorSelector
    {
        public bool AreAllVectorsSelected { get; }

        public AllVectorsSelector(bool areAllVectorsSelected)
        {
            AreAllVectorsSelected = areAllVectorsSelected;
        }
    }

    internal class IncludeNamedVectorsSelector : VectorSelector
    {
        public string[] IncludedVectorNames { get; }

        public IncludeNamedVectorsSelector(IEnumerable<string> includedVectorNames)
        {
            IncludedVectorNames = includedVectorNames.ToArray();
        }
    }

    /// <summary>
    /// Creates an instance of the vector selector that selects all vectors to be returned.
    /// </summary>
    public static VectorSelector All { get; } = new AllVectorsSelector(true);

    /// <summary>
    /// Creates an instance of the vector selector that selects none of the vectors to be returned.
    /// </summary>
    public static VectorSelector None { get; } = new AllVectorsSelector(false);

    /// <summary>
    /// Creates an instance of the payload properties selector that selects only specified payload properties to be returned.
    /// </summary>
    public static VectorSelector Include(params string[] includedVectorNames) =>
        new IncludeNamedVectorsSelector(includedVectorNames);

    /// <summary>
    /// Creates an instance of the payload properties selector that selects only specified payload properties to be returned.
    /// </summary>
    public static VectorSelector Include(IEnumerable<string> includedVectorNames) =>
        new IncludeNamedVectorsSelector(includedVectorNames);

    #region Operators

    /// <summary>
    /// Implicitly converts boolean values to <see cref="AllVectorsSelector"/> instances.
    /// </summary>
    /// <param name="value">If <c>true</c> - includes all vectors to the result.
    /// If <c>false</c> excludes all vectors.</param>
    public static implicit operator VectorSelector(bool value)
    {
        return new AllVectorsSelector(value);
    }

    #endregion
}
