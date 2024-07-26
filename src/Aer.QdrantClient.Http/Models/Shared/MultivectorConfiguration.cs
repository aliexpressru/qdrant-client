using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a multivector configuration.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class MultivectorConfiguration
{
    /// <summary>
    /// The comparator to be used while comparing multivectors.
    /// </summary>
    public required MultivectorComparator Comparator { set; get; }

    /// <summary>
    /// Initializes a new instance of <see cref="MultivectorConfiguration"/>.
    /// </summary>
    /// <param name="comparator">The comparator to be used with multivector component vectors.</param>
    [SetsRequiredMembers]
    public MultivectorConfiguration(MultivectorComparator comparator)
    {
        Comparator = comparator;
    }
}
