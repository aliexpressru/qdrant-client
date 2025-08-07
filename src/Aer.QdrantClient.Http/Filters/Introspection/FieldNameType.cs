using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Introspection;

/// <summary>
/// Represents a pair of payload field name and its corresponding qdrant type inferred from filter.
/// </summary>
/// <param name="Name">The name of the field.</param>
/// <param name="Type">The inferred type of the field.</param>
/// <remarks>Can be used for filter validation.</remarks>
public record FieldNameType(string Name, PayloadIndexedFieldType? Type);
