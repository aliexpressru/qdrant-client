using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Initializes a new instance of the <see cref="SliceCondition"/> class.
/// </summary>
/// <remarks>
/// One of total disjoint deterministic slices of the id space.
/// A point belongs to the slice iff hash(id) % total == index, where hash is SipHash-2-4 with a zero key over the canonical id bytes: 8 little-endian bytes for numeric ids, the 16 RFC 4122 bytes for UUIDs.
/// For a fixed total, slices 0..total are disjoint and together cover all points; membership is uniform regardless of the id scheme and stable across queries, segments, platforms and Qdrant versions.
/// Slices with different total values are correlated (same hash, no salt): e.g. slice 0 of total: 4 is a strict subset of slice 0 of total: 2. This keeps a smaller sample contained in a larger one.
/// </remarks>
/// <param name="total">Total number of disjoint slices the id space is split into.</param>
/// <param name="index">Which slice to select, must be in 0..total.</param>
internal sealed class SliceCondition(
    uint total,
    uint index) : FilterConditionBase(DiscardPayloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteObject("slice"))
        {
            jsonWriter.WriteNumber("total", total);
            jsonWriter.WriteNumber("index", index);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitSliceCondition(this);
}
