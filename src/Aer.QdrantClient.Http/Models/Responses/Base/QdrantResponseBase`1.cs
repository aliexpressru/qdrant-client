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
    /// Ensures that the <see cref="QdrantResponseBase.Status"/> indicates successful response and returns the <see cref="Result"/>.
    /// Throws <see cref="QdrantUnsuccessfulResponseStatusException"/> if it does not.
    /// </summary>
    /// <exception cref="QdrantUnsuccessfulResponseStatusException">Occurs when <see cref="QdrantResponseBase.Status"/> does not indicate success.</exception>
    public TResult EnsureSuccess()
    {
        if (Status.IsSuccess)
        {
            return Result;
        }

        throw new QdrantUnsuccessfulResponseStatusException(GetType(), Status);
    }
}
