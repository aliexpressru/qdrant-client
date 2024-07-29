using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointsQueryJsonConverter : JsonConverter<PointsQuery>
{
    public override PointsQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(PointsQuery)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, PointsQuery value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case PointsQuery.ContextQuery contextQuery:
                break;
            case PointsQuery.DiscoverPointsQuery discoverPointsQuery:
                break;
            case PointsQuery.FusionQuery fusionQuery:
                break;
            case PointsQuery.NearestPointsQuery nearestPointsQuery:
                break;
            case PointsQuery.OrderByQuery orderByQuery:
                break;
            case PointsQuery.RecommendPointsQuery recommendPointsQuery:
                break;
            case PointsQuery.SpecificPointQuery specificPointQuery:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}
