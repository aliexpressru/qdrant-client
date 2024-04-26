using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class OrderByStartFromJsonConverter : JsonConverter<OrderByStartFrom>
{
    public override OrderByStartFrom Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(OrderByStartFrom)} instances is not supported");
    }

    public override void Write(
        Utf8JsonWriter writer,
        OrderByStartFrom value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case OrderByStartFrom.OrderByStartFromInteger obsi:
                writer.WriteNumberValue(obsi.StartFrom);

                return;
            case OrderByStartFrom.OrderByStartFromDouble obsd:
                writer.WriteNumberValue(obsd.StartFrom);

                return;
            case OrderByStartFrom.OrderByStartFromDateTime obsdt:
                writer.WriteStringValue(obsdt.StartFrom.ToString("u"));
                break;
        }
    }
}
