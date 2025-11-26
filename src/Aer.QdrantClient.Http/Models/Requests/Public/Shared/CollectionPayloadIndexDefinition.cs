using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents one payload index definition.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class CollectionPayloadIndexDefinition
{
    /// <summary>
    /// The name of the payload property to index.
    /// </summary>
    public string PayloadIndexedFieldName { get; }

    /// <summary>
    /// The type of the indexed payload property.
    /// </summary>
    public PayloadIndexedFieldType PayloadIndexedFieldSchema { get; }

    /// <summary>
    /// If set to <c>true</c> the payload will be stored on-disk instead of in-memory.
    /// On-disk payload index might affect cold requests latency, as it requires additional disk I/O operations.
    /// </summary>
    public bool OnDisk { get; }

    /// <summary>
    /// Set to <c>true</c> to enable tenant index for specified payload field.
    /// </summary>
    public bool IsTenant { get; }

    /// <summary>
    /// Set to <c>true</c> to enable principal index for specified payload field.
    /// The principal index is used to optimize storage for faster search,
    /// assuming that the search request is primarily filtered by the principal field.
    /// </summary>
    public bool IsPrincipal { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionPayloadIndexDefinition"/> class.
    /// </summary>
    /// <param name="payloadIndexedFieldName">The indexed payload property name.</param>
    /// <param name="payloadIndexedFieldSchema">The type of the indexed payload property.</param>
    /// <param name="onDisk">
    /// If set to <c>true</c> the payload will be stored on-disk instead of in-memory.
    /// On-disk payload index might affect cold requests latency, as it requires additional disk I/O operations.
    /// </param>
    /// <param name="isTenant">Set to <c>true</c> to enable tenant index for specified payload field.</param>
    /// <param name="isPrincipal">
    /// Set to <c>true</c> to enable principal index for specified payload field.
    /// The principal index is used to optimize storage for faster search,
    /// assuming that the search request is primarily filtered by the principal field.
    /// </param>
    public CollectionPayloadIndexDefinition(
        string payloadIndexedFieldName,
        PayloadIndexedFieldType payloadIndexedFieldSchema,
        bool onDisk = false,
        bool isTenant = false,
        bool isPrincipal = false)
    {
        PayloadIndexedFieldName = payloadIndexedFieldName;
        PayloadIndexedFieldSchema = payloadIndexedFieldSchema;
        OnDisk = onDisk;
        IsPrincipal = isPrincipal;
        IsTenant = isTenant;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"\"{PayloadIndexedFieldName}\":{PayloadIndexedFieldSchema}(OnDisk: {OnDisk}, Tenant:{IsTenant}, Principal:{IsPrincipal})";
}
