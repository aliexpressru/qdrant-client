using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Infrastructure.Json;

internal static class JsonSerializerConstants
{
    private static readonly JTokenJsonConverter _jTokenJsonConverter = new();

    public static JsonNamingPolicy NamingStrategy { get; } = JsonNamingPolicy.SnakeCaseLower;

    public static JsonConverter EnumConverter { get; } = new JsonStringEnumConverter(NamingStrategy);

    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = NamingStrategy,
        Converters = {EnumConverter, _jTokenJsonConverter},
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.Strict,
        IgnoreReadOnlyProperties = false,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        IncludeFields = false,
        PropertyNameCaseInsensitive = true,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };

    public static JsonSerializerOptions IndentedSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = NamingStrategy,
        Converters = {EnumConverter, _jTokenJsonConverter},
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.Strict,
        IgnoreReadOnlyProperties = false,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        IncludeFields = false,
        PropertyNameCaseInsensitive = true,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };
}
