namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Which payload value to start scrolling from. Default is the lowest value for asc and the highest for desc.
/// </summary>
public abstract class OrderByStartFrom
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByStartFrom"/> class with integer start.
    /// </summary>
    /// <param name="startFrom">The starting value for the order by.</param>
    internal sealed class OrderByStartFromInteger(int startFrom) : OrderByStartFrom
    {
        /// <summary>
        /// Which payload value to start scrolling from.
        /// </summary>
        public int StartFrom { get; } = startFrom;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByStartFrom"/> class with double start.
    /// </summary>
    /// <param name="startFrom">The starting value for the order by.</param>
    internal sealed class OrderByStartFromDouble(double startFrom) : OrderByStartFrom
    {
        /// <summary>
        /// Which payload value to start scrolling from.
        /// </summary>
        public double StartFrom { get; } = startFrom;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByStartFrom"/> class with date-time start.
    /// </summary>
    /// <param name="startFrom">The starting value for the order by.</param>
    internal sealed class OrderByStartFromDateTime(DateTimeOffset startFrom) : OrderByStartFrom
    {
        /// <summary>
        /// Which payload value to start scrolling from.
        /// </summary>
        public DateTimeOffset StartFrom { get; } = startFrom;
    }

    /// <summary>
    /// Implicitly converts the integer value to <see cref="OrderByStartFrom"/>.
    /// </summary>
    /// <param name="startFrom">The start from value.</param>
    public static implicit operator OrderByStartFrom(int startFrom) => new OrderByStartFromInteger(startFrom);

    /// <summary>
    /// Implicitly converts the double value to <see cref="OrderByStartFrom"/>.
    /// </summary>
    /// <param name="startFrom">The start from value.</param>
    public static implicit operator OrderByStartFrom(double startFrom) => new OrderByStartFromDouble(startFrom);

    /// <summary>
    /// Implicitly converts the date-time value to <see cref="OrderByStartFrom"/>.
    /// </summary>
    /// <param name="startFrom">The start from value.</param>
    public static implicit operator OrderByStartFrom(DateTimeOffset startFrom) => new OrderByStartFromDateTime(startFrom);
}
