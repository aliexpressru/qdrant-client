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
    Datetime,

    /// <summary>
    /// The fulltext type. Used only for a fulltext indexes.
    /// </summary>
    Text,

    /// <summary>
    /// The UUID type. Functionally, it works the same as keyword, internally stores parsed UUID values.
    /// Usage of uuid index type is recommended in payload-heavy collections to save RAM and improve search performance.
    /// </summary>
    Uuid
}
