using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// The base class for all the Qdrant API responses with result.
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
    public TResult Result { get; init; }

    /// <summary>
    /// Ensures that the <see cref="QdrantResponseBase.Status"/> indicates successfull response.
    /// Throws <see cref="QdrantUnsuccessfulResponseStatusException"/> if it does not.
    /// </summary>
    /// <exception cref="QdrantUnsuccessfulResponseStatusException">Occurs when <see cref="QdrantResponseBase.Status"/> does not indicate success.</exception>
    public override QdrantResponseBase<TResult> EnsureSuccess()
    {
        if (Status.IsSuccess)
        {
            return this;
        }

        throw new QdrantUnsuccessfulResponseStatusException(GetType(), Status);
    }
}
