using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// The base class for all the Qdrant API responses with result.
/// </summary>
/// <typeparam name="TResult">The type of the class into which the <c>result</c> response json property should be deserialized.</typeparam>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public abstract class QdrantResponseBase<TResult> : QdrantResponseBase
{
    /// <summary>
    /// The response result.
    /// </summary>
    public TResult Result { get; init; }

    /// <summary>
    /// Constructs a new instance of <see cref="QdrantResponseBase{TResult}"/>.
    /// </summary>
    protected QdrantResponseBase()
    { }

    /// <summary>
    /// Used to copy the base properties (<see cref="QdrantResponseBase.Status"/>, <see cref="QdrantResponseBase.Usage"/>, <see cref="QdrantResponseBase.Time"/>) from another response.
    /// Primarily used to propagate operation status and statistics in case of errors in compound operations.
    /// </summary>
    /// <param name="childResponse">This child response to copy base properties from</param>
    protected internal QdrantResponseBase(QdrantResponseBase childResponse)
    { 
        Status = childResponse.Status;
        Usage = childResponse.Usage;
        Time = childResponse.Time;
    }

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
    
    /// <summary>
    /// Fills the response stats (<see cref="QdrantResponseBase.Status"/>, <see cref="QdrantResponseBase.Usage"/>, <see cref="QdrantResponseBase.Time"/>) from another response.
    /// </summary>
    public QdrantResponseBase FillResponseStatsFromResponse(QdrantResponseBase response)
    {
        if (response is null)
        {
            return this;
        }

        Status = response.Status;
        Time = response.Time;
        Usage = response.Usage;
        
        return this;
    }
}
