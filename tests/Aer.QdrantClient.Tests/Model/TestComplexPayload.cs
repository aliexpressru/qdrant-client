using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Tests.Model;

internal class TestComplexPayload : Payload
{
    public string Text { get; set; }

    public int? IntProperty { get; set; }

    public double? FloatingPointNumber { get; set; }

    public int[] Array { set; get; }

    public GeoPoint Location { get; set; }

    public NestedClass Nested { get; set; }

    [JsonPropertyName("name_override")]
    public string TestOverriddenPropertyName { get; set; }

    public class NestedClass
    {
        public string Name { get; set; }

        public double Double { set; get; }

        public int Integer { get; set; }
    }
}
