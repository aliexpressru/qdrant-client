// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global

using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents the geo point data.
/// </summary>
public class GeoPoint
{
    /// <summary>
    /// The longtitude.
    /// </summary>
    public required double Longtitude { get; set; }

    /// <summary>
    /// The latitude.
    /// </summary>
    public required double Latitude { get; set; }

    /// <summary>
    /// Initializes new instance of <see cref="GeoPoint"/> with given coordinates.
    /// </summary>
    /// <param name="latitude">The point latitude.</param>
    /// <param name="longtitude">The point longtitude.</param>
    [SetsRequiredMembers]
    public GeoPoint(double latitude, double longtitude)
    {
        Longtitude = longtitude;
        Latitude = latitude;
    }
}
