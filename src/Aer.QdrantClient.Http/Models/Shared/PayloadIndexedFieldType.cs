// ReSharper disable UnusedMember.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The type of the indexed payload property.
/// </summary>
public enum PayloadIndexedFieldType
{
    /// <summary>
    /// The keyword (string) type.
    /// </summary>
    Keyword,

    /// <summary>
    /// The integer number type.
    /// </summary>
    Integer,

    /// <summary>
    /// The floating point number type.
    /// </summary>
    Float,

    /// <summary>
    /// The geo coordinates type.
    /// </summary>
    Geo,

    /// <summary>
    /// The datetime type.
    /// </summary>
    Datetime
}
