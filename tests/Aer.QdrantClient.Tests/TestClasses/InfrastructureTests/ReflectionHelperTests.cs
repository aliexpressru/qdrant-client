using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class ReflectionHelperTests
{
    [Test]
    public void TestReflectionHelper_GetSimpleSingleWordNameProperty()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t)=>t.Text);
        payloadMemberName.Should().Be("text");
    }

    [Test]
    public void TestReflectionHelper_GetSimpleMultiWordNameProperty()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.IntProperty);
        payloadMemberName.Should().Be("int_property");
    }

    [Test]
    public void TestReflectionHelper_GetNestedSimpleMultiWordNameProperty()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.Nested.Name);
        payloadMemberName.Should().Be("nested.name");
    }

    [Test]
    public void TestReflectionHelper_GetOverriddenPropertyName()
    {
        var payloadMemberName = ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.TestOverriddenPropertyName);
        payloadMemberName.Should().Be("name_override");
    }
}
