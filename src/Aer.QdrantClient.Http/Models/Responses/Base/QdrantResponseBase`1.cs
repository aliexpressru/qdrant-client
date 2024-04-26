using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// The base class for all the Qdrant API responses.
/// </summary>
/// <typeparam name="TResult">The type of the class into which the <c>result</c> response json property should be deserialized.</typeparam>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class QdrantResponseBase<TResult> : QdrantResponseBase
{
    /// <summary>
    /// The response result.
    /// </summary>
    public TResult Result { get; set; }
}
