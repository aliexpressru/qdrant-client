using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Tests.Helpers;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class PayloadTests
{
    [Test]
    public void Deserialization_EmptyPayload()
    {
        var emptyPayload = new Payload(Payload.EmptyString);
        emptyPayload.IsEmpty.Should().BeTrue();

        var emptyPayloadDeserializationAct = () => GetPayloadObject(
            emptyPayload,
            () => new
            {
                Name = default(string),
                // ReSharper disable once PreferConcreteValueOverDefault
                Age = default(int)
            }
        );

        emptyPayloadDeserializationAct.Should().Throw<InvalidOperationException>();

        var emptyObject = GetPayloadObject(
            emptyPayload,
            () => new
            {
                Name = default(string),
                // ReSharper disable once PreferConcreteValueOverDefault
                Age = default(int)
            },
            shouldThrowIfEmpty: false
        );

        emptyObject.Should().BeNull();
    }

    [Test]
    public void Deserialization()
    {
        var payloadString = """
            {
                "Name" : "test",
                "Age" : 30 
            }
            """;

        var payload = new Payload(payloadString);

        payload.RawPayload.Should().NotBeNull();
        payload.ToString(isFormatPayloadJson: true).AssertSameString(payloadString);

        // Using a bit hacky thing, but I can't be bothered to create a separate class for this test
        var payloadObject = GetPayloadObject(
            payload,
            () => new
            {
                Name = default(string),
                // ReSharper disable once PreferConcreteValueOverDefault
                Age = default(int)
            }
        );

        payloadObject.Should().NotBeNull();

        payloadObject.Name.Should().Be("test");
        payloadObject.Age.Should().Be(30);
    }

    [Test]
    public void GetField_NonExistent()
    {
        var payload = new Payload(
            """
            {
                "name" : "test",
                "age" : 30 
            }
            """);

        payload.ContainsField("non_existent_field").Should().BeFalse();

        payload.TryGetValue<string>("non_existent_field", out var value).Should().BeFalse();
        value.Should().BeNull();

        payload.TryGetValue<string>("nested.field", out var value2).Should().BeFalse();
        value2.Should().BeNull();

        var getValueAct = () => payload.GetValue<string>("non_existent_field");
        getValueAct.Should().Throw<KeyNotFoundException>();

        var getRawValueAct = () => payload["non_existent_field"];
        getRawValueAct.Should().Throw<KeyNotFoundException>();

        var getRawNestedValueAct = () => payload["nested.field"];
        getRawNestedValueAct.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void GetField()
    {
        var payload = new Payload(
            """
            {
                "name" : "test",
                "age" : 30 
            }
            """);

        payload.ContainsField("name").Should().BeTrue();
        payload.TryGetValue<string>("name", out var name).Should().BeTrue();
        name.Should().Be("test");
        payload.GetValue<string>("name").Should().Be("test");

        payload["name"].Should().NotBeNull();
        payload["name"]!.ToString().Should().Be("test");

        payload.ContainsField("age").Should().BeTrue();
        payload.TryGetValue<int>("age", out var age).Should().BeTrue();
        age.Should().Be(30);
        payload.GetValue<int>("age").Should().Be(30);

        payload["age"].Should().NotBeNull();
        payload["age"]!.ToString().Should().Be("30");
    }

#pragma warning disable IDE0060 // Remove unused parameter | Justification : The parameter is used to help with type inference
    private static T GetPayloadObject<T>(Payload payload, Func<T> typeBuilder, bool shouldThrowIfEmpty = true)
#pragma warning restore IDE0060 // Remove unused parameter
        where T : class
        =>
            payload.As<T>(shouldThrowIfEmpty);
}
