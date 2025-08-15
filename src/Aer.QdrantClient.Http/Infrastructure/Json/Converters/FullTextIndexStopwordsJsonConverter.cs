using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class FullTextIndexStopwordsJsonConverter : JsonConverter<FullTextIndexStopwords>
{
    public override FullTextIndexStopwords Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                // Means default words list which is described by language name only
                
                var languageName = reader.GetString();

                if (Enum.TryParse<StopwordsLanguage>(languageName, ignoreCase: true, out var language))
                {
                    return new FullTextIndexStopwords.DefaultStopwords(language);
                }

                throw new QdrantJsonValueParsingException($"stop words list language name '{languageName}'");

            case JsonTokenType.StartObject:
                // Means a custom words list which is described by an object 
                /*
                 "stopwords": {
                        "languages": [
                            "english",
                            "spanish"
                        ],
                        "custom": [
                            "example"
                        ]
                    }
                 */
                var customStopwordsObject = JsonElement.ParseValue(ref reader);
                var customStopwordsSet = customStopwordsObject.Deserialize<FullTextIndexStopwords.CustomStopwordsSet>(
                        JsonSerializerConstants.DefaultSerializerOptions)
                    ?? throw new QdrantJsonValueParsingException("Failed to deserialize custom stopwords set");

                return customStopwordsSet;

            default:
                throw new QdrantJsonValueParsingException(reader.GetString());
        }
    }

    public override void Write(Utf8JsonWriter writer, FullTextIndexStopwords value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case FullTextIndexStopwords.DefaultStopwords dsw:
                writer.WriteStringValue(dsw.Language.ToString().ToLowerInvariant());

                return;
            case FullTextIndexStopwords.CustomStopwordsSet csw:
                JsonSerializer.Serialize(writer, csw, JsonSerializerConstants.DefaultSerializerOptions);

                return;
        }
    }
}
