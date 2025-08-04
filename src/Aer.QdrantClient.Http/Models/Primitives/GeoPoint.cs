using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents the geo point data.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class GeoPoint
{
    /// <summary>
    /// The longitude.
    /// </summary>
    public required double Longitude { get; init; }

    /// <summary>
    /// The latitude.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Initializes new instance of <see cref="GeoPoint"/> with given coordinates.
    /// </summary>
    /// <param name="latitude">The point latitude.</param>
    /// <param name="longitude">The point longitude.</param>
    [SetsRequiredMembers]
    public GeoPoint(double latitude, double longitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
}
