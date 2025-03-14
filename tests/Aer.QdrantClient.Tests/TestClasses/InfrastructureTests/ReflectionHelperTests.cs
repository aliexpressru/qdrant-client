using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class ReflectionHelperTests
{
    [Test]
    public void GetSimpleSingleWordNameProperty()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t)=>t.Text);
        payloadMemberName.Should().Be("text");
    }

    [Test]
    public void GetSimpleMultiWordNameProperty()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.IntProperty);
        payloadMemberName.Should().Be("int_property");
    }

    [Test]
    public void GetNestedSimpleMultiWordNameProperty()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.Nested.Name);
        payloadMemberName.Should().Be("nested.name");
    }

    [Test]
    public void GetOverriddenPropertyName()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.TestOverriddenPropertyName);
        payloadMemberName.Should().Be("name_override");
    }

    [Test]
    public void GetArrayPropertyName()
    {
        var payloadMemberName =
            ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.Array);
        
        payloadMemberName.Should().Be("array[]");
    }

    [Test]
    public void GetNestedInArrayPropertyName()
    {
        var payloadMemberName =
            ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.NestedArray[0].Name);
        
        payloadMemberName.Should().Be("nested_array[].name");
    }
}
