using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// Represents
/// </summary>
public abstract class QdrantResponseBase
{
    /// <summary>
    /// Contains the string or object describing the operation status.
    /// </summary>
    [JsonConverter(typeof(QdrantStatusJsonConverter))]
    public QdrantStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the time (in seconds) elapsed for the opeartion.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Ensures that the <see cref="Status"/> indicates successfull response.
    /// Throws <see cref="QdrantUnsuccessfullResponseStatusException"/> if it does not.
    /// </summary>
    /// <exception cref="QdrantUnsuccessfullResponseStatusException">Occures when <see cref="Status"/> does not indicate success.</exception>
    public void EnsureSuccess()
    {
        if (Status.IsSuccess)
        {
            return;
        }

        throw new QdrantUnsuccessfullResponseStatusException(GetType(), Status);
    }
}
