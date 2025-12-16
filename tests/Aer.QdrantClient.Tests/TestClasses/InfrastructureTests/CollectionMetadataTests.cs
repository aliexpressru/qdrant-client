using Aer.QdrantClient.Http.Models.Primitives;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class CollectionMetadataTests
{
    [Test]
    public void TypeConversion()
    {
        var metadata = new CollectionMetadata(new Dictionary<string, JsonElement>
        {
            ["int"] = JsonSerializer.SerializeToElement(42),
            ["string"] = JsonSerializer.SerializeToElement("test"),
            ["bool"] = JsonSerializer.SerializeToElement(true),
            ["datetime"] = JsonSerializer.SerializeToElement(DateTime.UtcNow),
            ["object"] = JsonSerializer.SerializeToElement(new { Name = "test", Value = 42 }),
            ["float"] = JsonSerializer.SerializeToElement(3.14f),
            ["double"] = JsonSerializer.SerializeToElement(3.14),

            ["test"] = JsonSerializer.SerializeToElement(new List<int> { 1, 2, 3 }),

            ["empty"] = JsonSerializer.SerializeToElement(new List<int>())
        });

        metadata.GetValueOrDefault("int", 0).Should().Be(42);
        metadata.GetValueOrDefault("string", "").Should().Be("test");
        metadata.GetValueOrDefault("bool", false).Should().Be(true);
        metadata.GetValueOrDefault("float", 0f).Should().BeApproximately(3.14f, 0.0001f);
        metadata.GetValueOrDefault("double", 0.0).Should().BeApproximately(3.14, 0.0001);
        metadata.GetValueOrDefault("datetime", DateTime.MinValue).Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        metadata.GetValueOrDefault("object", new { Name = "", Value = 0 }).Should().BeEquivalentTo(new { Name = "test", Value = 42 });

        // Try to get int as different type. Should circumvent cache and parse again.
        metadata.GetValueOrDefault<JsonElement>("int").Should().BeOfType<JsonElement>();
        metadata.GetValueOrDefault<JsonElement>("test").Should().BeOfType<JsonElement>();

        var jObject = metadata.GetValueOrDefault<JObject>("object");

        jObject["Name"]!.Value<string>().Should().Be("test");
        jObject["Value"]!.Value<int>().Should().Be(42);

        metadata.GetValueOrDefault<string[]>("empty", []).Should().BeEquivalentTo([]);

        metadata.GetValueOrDefault<string[]>("non-existent", null).Should().BeNull();
    }
}
