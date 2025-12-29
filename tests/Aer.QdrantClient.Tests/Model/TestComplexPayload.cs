using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Tests.Model;

internal class TestComplexPayload
{
    public string Text { get; set; }

    public int? IntProperty { get; set; }

    public double? FloatingPointNumber { get; set; }

    public int[] Array { set; get; }

    public string[] StringArray { set; get; }

    public DateTime? Date { set; get; }

    public Guid? Guid { set; get; }

    public GeoPoint Location { get; set; }

    public NestedClass Nested { get; set; }

    public NestedClass[] NestedArray { get; set; }

    [JsonPropertyName("name_override")]
    public string TestOverriddenPropertyName { get; set; }

    public class NestedClass
    {
        public string Name { get; set; }

        public double Double { set; get; }

        public int Integer { get; set; }

        public NestedNestedClass[] NestedNestedArray { get; set; }

        public NestedNestedClass Nested { get; set; }
    }

    public class NestedNestedClass
    {
        public string Name { get; set; }

        public double Double { set; get; }
    }
}
