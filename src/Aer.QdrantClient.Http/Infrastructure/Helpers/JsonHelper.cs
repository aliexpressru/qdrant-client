using System.Text.Json;

namespace Aer.QdrantClient.Http.Infrastructure.Helpers;

internal static class JsonHelper
{
    private enum JsonObjectType
    {
        Object,
        Array
    }

    private struct ObjectWriter(Utf8JsonWriter writer, JsonObjectType objectType) : IDisposable
    {
        public readonly void WriteStart()
        {
            switch (objectType)
            {
                case JsonObjectType.Object:
                    writer.WriteStartObject();
                    break;
                case JsonObjectType.Array:
                    writer.WriteEndArray();
                    break;
                default:
                    throw new InvalidOperationException("Unknown object type");

            }
        }

        public readonly void WriteEnd()
        {
            switch (objectType)
            {
                case JsonObjectType.Object:
                    writer.WriteEndObject();
                    break;
                case JsonObjectType.Array:
                    writer.WriteEndArray();
                    break;
                default:
                    throw new InvalidOperationException("Unknown object type");

            }
        }

        public readonly void Dispose() => WriteEnd();
    }

    extension(Utf8JsonWriter jsonWriter)
    {
        public void WriteEmptyObject()
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteEndObject();
        }

        public IDisposable WriteObject(string propertyName = null)
        {
            if (propertyName is not null)
            {
                jsonWriter.WritePropertyName(propertyName);
            }

            var objectWriter = new ObjectWriter(jsonWriter, JsonObjectType.Object);
            jsonWriter.WriteStartObject();

            return objectWriter;
        }

        public IDisposable WriteArray(string propertyName = null)
        {
            if (propertyName is not null)
            {
                jsonWriter.WritePropertyName(propertyName);
            }

            var objectWriter = new ObjectWriter(jsonWriter, JsonObjectType.Array);
            jsonWriter.WriteStartArray();

            return objectWriter;
        }
    }
}
