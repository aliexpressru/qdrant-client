using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// The request for creating full-text index on a specified text payload field.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal sealed class CreateFullTextPayloadIndexRequest
{
    /// <summary>
    /// Represents the full text payload field schema.
    /// </summary>
    public sealed class FullTextPayloadFieldSchema
    {
        /// <summary>
        /// The type of the payload field. Since this is a full-text index the field type can only be <c>Keyword</c>.
        /// </summary>
        public PayloadIndexedFieldType Type => PayloadIndexedFieldType.Text;

        /// <summary>
        /// The type of the payload text tokenizer.
        /// </summary>
        public PayloadIndexedTextFieldTokenizerType Tokenizer { get; set; }

        /// <summary>
        /// The minimal token length.
        /// </summary>
        public uint? MinTokenLen { get; set; }

        /// <summary>
        /// The maximal token length.
        /// </summary>
        public uint? MaxTokenLen { get; set; }

        /// <summary>
        /// If <c>true</c>, lowercase all tokens. Default: <c>true</c>.
        /// </summary>
        public bool Lowercase { get; set; } = true;

        /// <summary>
        /// If <c>true</c>, store the index on disk. Default: <c>false</c>.
        /// </summary>
        public bool OnDisk { get; set; } = false;
    }

    /// <summary>
    /// Gets or sets the name of the indexed field.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the schema of the indexed field.
    /// </summary>
    public FullTextPayloadFieldSchema FieldSchema { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePayloadIndexRequest"/> class.
    /// </summary>
    /// <param name="payloadTextFieldName">The name of the indexed text payload field.</param>
    /// <param name="payloadTextFieldSchema">The schema type of the indexed text payload field.</param>
    public CreateFullTextPayloadIndexRequest(
        string payloadTextFieldName,
        FullTextPayloadFieldSchema payloadTextFieldSchema)
    {
        FieldName = payloadTextFieldName;
        FieldSchema = payloadTextFieldSchema;
    }
}
