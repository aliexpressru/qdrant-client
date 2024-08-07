using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// The request for creating index on a payload field.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class CreatePayloadIndexRequest
{
    /// <summary>
    /// Gets or sets the name of the indexed field.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the schema type of the indexed field.
    /// </summary>
    public string FieldSchema { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePayloadIndexRequest"/> class.
    /// </summary>
    /// <param name="payloadFieldName">The name of the indexed payload field.</param>
    /// <param name="payloadFieldType">The type of the indexed payload field.</param>
    public CreatePayloadIndexRequest(string payloadFieldName, PayloadIndexedFieldType payloadFieldType)
    {
        FieldName = payloadFieldName;
        FieldSchema = payloadFieldType.ToString().ToLowerInvariant();
    }
}
