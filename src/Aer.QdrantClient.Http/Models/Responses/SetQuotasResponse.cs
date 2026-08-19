using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of setting quotas.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SetQuotasResponse : QdrantResponseBase<bool>
{
    /// <summary>
    /// Creates a new instance of <see cref="SetQuotasResponse"/>.
    /// </summary>
    public SetQuotasResponse()
    { }

    internal SetQuotasResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }
}