using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a collection alias information.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class CollectionAlias
{
    /// <summary>
    /// The collection alias.
    /// </summary>
    public string AliasName { set; get; }

    /// <summary>
    /// The collection original name.
    /// </summary>
    public string CollectionName { set; get; }
}
