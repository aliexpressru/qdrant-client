using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a multivector configuration.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class MultivectorConfiguration
{
    /// <summary>
    /// The comparator to be used while comparing multivectors.
    /// </summary>
    [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<MultivectorComparator>))]
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
