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
    /// <remarks>
    /// Datetime should be in RFC 3339 https://datatracker.ietf.org/doc/html/rfc3339#section-5.6 format.
    /// For more information, see the <a href="https://qdrant.tech/documentation/concepts/payload/#datetime">Qdrant datetime payload type documentation</a>.
    /// 
    /// Supported formats:
    /// <li>
    /// "2023-02-08T10:49:00Z" (RFC 3339, UTC)
    /// </li>
    /// 
    /// <li>
    /// "2023-02-08T11:49:00+01:00"  (RFC 3339, with timezone)
    /// </li>
    /// 
    /// <li>
    /// "2023-02-08T10:49:00" (without timezone, UTC is assumed)
    /// </li>
    /// 
    /// <li>
    /// "2023-02-08T10:49" (without timezone and seconds)
    /// </li>
    /// 
    /// <li>
    /// "2023-02-08" (only date, midnight is assumed)
    /// </li>
    /// </remarks>
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
