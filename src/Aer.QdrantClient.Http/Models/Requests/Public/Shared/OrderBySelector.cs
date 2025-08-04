using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// The selector for the field that the results should be ordered by.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class OrderBySelector
{
    /// <summary>
    /// Payload key to order by.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Direction of ordering: ascending or descending. Default is ascending.
    /// </summary>
    public OrderByDirection Direction { get; }

    /// <summary>
    /// The starting value for the order by.
    /// </summary>
    [JsonConverter(typeof(OrderByStartFromJsonConverter))]
    public OrderByStartFrom StartFrom { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderBySelector"/> class with the specified payload field and order direction.
    /// </summary>
    /// <param name="payloadField">The payload field to order by.</param>
    /// <param name="direction">The direction of ordering. Default is <see cref="OrderByDirection.Asc"/>.</param>
    /// <param name="startFrom">The starting value for the order by.</param>
    private OrderBySelector(
        string payloadField,
        OrderByDirection direction,
        OrderByStartFrom startFrom)
    {
        Key = payloadField;
        Direction = direction;
        StartFrom = startFrom;
    }

    /// <summary>
    /// Creates an instance of the <see cref="OrderBySelector"/> class
    /// with the specified payload field name and default ascending order.
    /// </summary>
    /// <param name="payloadFieldName">The name of the payload field to order by.</param>
    /// <param name="startFrom">The starting value for the order by.</param>
    public static OrderBySelector Asc(string payloadFieldName, OrderByStartFrom startFrom = null)
        => new(payloadFieldName, OrderByDirection.Asc, startFrom);

    /// <summary>
    /// Creates an instance of the <see cref="OrderBySelector"/> class
    /// with the specified payload field name and descending order.
    /// </summary>
    /// <param name="payloadFieldName">The name of the payload field to order by.</param>
    /// <param name="startFrom">The starting value for the order by.</param>
    public static OrderBySelector Desc(string payloadFieldName, OrderByStartFrom startFrom = null)
        => new(payloadFieldName, OrderByDirection.Desc, startFrom);

    /// <summary>
    /// Implicit conversion from string to <see cref="OrderBySelector"/> with ascending order.
    /// </summary>
    /// <param name="payloadFieldName">The payload field</param>
    public static implicit operator OrderBySelector(string payloadFieldName)
        => new(payloadFieldName, OrderByDirection.Asc, null);
}
