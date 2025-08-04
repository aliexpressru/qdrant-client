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
    /// Represents a field schema configuration.
    /// </summary>
    internal sealed class FieldSchemaUnit(
        string type,
        bool onDisk,
        bool isTenant,
        bool isPrincipal)
    {
        /// <summary>
        /// The type of the indexed payload field.
        /// </summary>
        public string Type { get; } = type;

        /// <summary>
        /// Set to <c>true</c> to store specified payload field index on-disk instead of in-memory.
        /// </summary>
        public bool OnDisk { get; } = onDisk;

        /// <summary>
        /// Set to <c>true</c> to enable tenant index for specified payload field.
        /// </summary>
        public bool IsTenant { get; } = isTenant;

        /// Set to <c>true</c> to enable principal index for specified payload field.
        public bool IsPrincipal { get; } = isPrincipal;
    }

    /// <summary>
    /// Gets or sets the name of the indexed field.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the schema type of the indexed field.
    /// </summary>
    public FieldSchemaUnit FieldSchema { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePayloadIndexRequest"/> class.
    /// </summary>
    /// <param name="payloadFieldName">The name of the indexed payload field.</param>
    /// <param name="payloadFieldType">The type of the indexed payload field.</param>
    /// <param name="onDisk">Whether to store index on-disk instead of in-memory.</param>
    /// <param name="isTenant">Set to <c>true</c> to enable tenant index for specified payload field.</param>
    /// <param name="isPrincipal">
    /// Set to <c>true</c> to enable principal index for specified payload field.
    /// The principal index is used to optimize storage for faster search,
    /// assuming that the search request is primarily filtered by the principal field.
    /// </param>
    public CreatePayloadIndexRequest(
        string payloadFieldName,
        PayloadIndexedFieldType payloadFieldType,
        bool onDisk,
        bool isTenant,
        bool isPrincipal)
    {
        FieldName = payloadFieldName;
        FieldSchema = new FieldSchemaUnit(
            type: payloadFieldType.ToString().ToLowerInvariant(),
            onDisk: onDisk,
            isTenant: isTenant,
            isPrincipal: isPrincipal
        );
    }
}
