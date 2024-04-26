using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Infrastructure.Json;

internal static class JsonSerializerConstants
{
    public static readonly JsonNamingPolicy NamingStrategy = JsonNamingPolicy.SnakeCaseLower;

    public static readonly JsonConverter EnumConverter = new JsonStringEnumConverter(NamingStrategy);

    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = NamingStrategy,
        Converters = {EnumConverter, new ByteArrayJsonConverter()},
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.Strict,
        IgnoreReadOnlyProperties = false,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        IncludeFields = false,
        PropertyNameCaseInsensitive = true,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    };

    public static JsonSerializerOptions IndentedSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = NamingStrategy,
        Converters = {EnumConverter, new ByteArrayJsonConverter()},
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
