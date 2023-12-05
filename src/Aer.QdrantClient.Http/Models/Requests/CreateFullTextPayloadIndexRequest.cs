using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// The request for creating full-text index on a specified text payload field.
/// </summary>
internal sealed class CreateFullTextPayloadIndexRequest
{
    #region Nested types

    /// <summary>
    /// Represents the full text payload field schema.
    /// </summary>
    public class FullTextPayloadFieldSchema
    {
        /// <summary>
        /// The type of the payload field. Since this is a full-text index the field type can only be <c>Keyword</c>.
        /// </summary>
        public PayloadIndexedFieldType Type { get; } = PayloadIndexedFieldType.Keyword;

        /// <summary>
        /// The type of the payloiad text tokenizer.
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
    }

    #endregion

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
    public CreateFullTextPayloadIndexRequest(string payloadTextFieldName, FullTextPayloadFieldSchema payloadTextFieldSchema)
    {
        FieldName = payloadTextFieldName;
        FieldSchema = payloadTextFieldSchema;
    }
}
