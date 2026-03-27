using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace Aer.QdrantClient.Http.Infrastructure.Json;

internal static class JsonSerializerConstants
{
    private static readonly JTokenJsonConverter _jTokenJsonConverter = new();

    public static JsonNamingPolicy NamingStrategy => JsonNamingPolicy.SnakeCaseLower;

    public static JsonConverter EnumConverter { get; } = new JsonStringSnakeCaseLowerEnumConverter();

    public static JsonSerializerOptions DefaultSerializerOptions { get; } = CreateSerializerOptions();

    public static JsonSerializerOptions DefaultIndentedSerializerOptions { get; } =
        CreateSerializerOptions(o => o.WriteIndented = true);

    public static JsonSerializerOptions CreateSerializerOptions(params JsonConverter[] additionalConverters)
        => CreateSerializerOptions(null, additionalConverters);

    public static JsonSerializerOptions CreateSerializerOptions(
        Action<JsonSerializerOptions> configure,
        params JsonConverter[] additionalConverters)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = false,
            Converters = { EnumConverter, _jTokenJsonConverter },
            PropertyNamingPolicy = NamingStrategy,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.Strict,
            IgnoreReadOnlyProperties = false,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,

            TypeInfoResolver = SourceGenerationContext.Default
        };

        configure?.Invoke(options);

        if (additionalConverters is { Length: > 0 } existingAdditionalConverters)
        {
            foreach (var additionalConverter in existingAdditionalConverters)
            {
                options.Converters.Add(additionalConverter);
            }
        }

        return options;
    }
}
