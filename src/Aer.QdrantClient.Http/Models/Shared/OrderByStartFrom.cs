namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Which payload value to start scrolling from. Default is the lowest value for asc and the highest for desc.
/// </summary>
public class OrderByStartFrom
{
    internal class OrderByStartFromInteger : OrderByStartFrom
    {
        /// <summary>
        /// Which payload value to start scrolling from.
        /// </summary>
        public int StartFrom { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByStartFrom"/> class with integer start.
        /// </summary>
        /// <param name="startFrom">The starting value for the order by.</param>
        public OrderByStartFromInteger(int startFrom)
        {
            StartFrom = startFrom;
        }
    }

    internal class OrderByStartFromDouble : OrderByStartFrom
    {
        /// <summary>
        /// Which payload value to start scrolling from.
        /// </summary>
        public double StartFrom { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByStartFrom"/> class with double start.
        /// </summary>
        /// <param name="startFrom">The starting value for the order by.</param>
        public OrderByStartFromDouble(double startFrom)
        {
            StartFrom = startFrom;
        }
    }

    internal class OrderByStartFromDateTime : OrderByStartFrom
    {
        /// <summary>
        /// Which payload value to start scrolling from.
        /// </summary>
        public DateTime StartFrom { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByStartFrom"/> class with date-time start.
        /// </summary>
        /// <param name="startFrom">The starting value for the order by.</param>
        public OrderByStartFromDateTime(DateTime startFrom)
        {
            StartFrom = startFrom;
        }
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
    public static implicit operator OrderByStartFrom(DateTime startFrom) => new OrderByStartFromDateTime(startFrom);
}
