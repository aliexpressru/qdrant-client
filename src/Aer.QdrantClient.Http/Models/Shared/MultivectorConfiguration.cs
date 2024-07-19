using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a multivector configuration.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class MultivectorConfiguration
{
    /// <summary>
    /// The comparator to be used while comparing multivectors.
    /// </summary>
    public required MultivectorComparator Comparator { set; get; }
}
