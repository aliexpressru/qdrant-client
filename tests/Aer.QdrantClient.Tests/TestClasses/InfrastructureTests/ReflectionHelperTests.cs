using System.Linq.Expressions;
using System.Threading.RateLimiting;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Tests.Model;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;

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
        
        payloadMemberName.Should().Be("array");
    }

    [Test]
    public void GetNestedInArrayPropertyName()
    {
        var payloadMemberName =
            ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.NestedArray[0].Name);
        
        payloadMemberName.Should().Be("nested_array[].name");
    }

    [Test]
    public void GetNestedArrayPropertyName_DoublyNested()
    {
        var payloadMemberName =
            ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.NestedArray[0].NestedNestedArray);

        payloadMemberName.Should().Be("nested_array[].nested_nested_array");
    }

    [Test]
    public void GetNestedInArrayPropertyName_DoublyNested()
    {
        var payloadMemberName =
            ReflectionHelper.GetPayloadFieldName((TestComplexPayload t) => t.NestedArray[0].NestedNestedArray[0].Name);

        payloadMemberName.Should().Be("nested_array[].nested_nested_array[].name");
    }

    [Test]
    public void CheckRetryStrategyConfigured()
    {
        var delay = TimeSpan.FromMilliseconds(200);

        Expression<Action<ResiliencePipelineBuilder<HttpResponseMessage>>> resilienceStrategyConfiguredAction =
            builder =>
                builder
                    .AddConcurrencyLimiter(new ConcurrencyLimiterOptions(){
                        PermitLimit = 2
                    })
                    .AddRetry(
                        new HttpRetryStrategyOptions()
                        {
                            MaxRetryAttempts = 3,
                            Delay = delay
                        })
                    .AddCircuitBreaker(
                        new CircuitBreakerStrategyOptions<HttpResponseMessage>()
                        {
                            FailureRatio = 1
                        });

        var isConfigured = ReflectionHelper.CheckRetryStrategyConfigured(resilienceStrategyConfiguredAction);
        isConfigured.Should().BeTrue();

        Expression<Action<ResiliencePipelineBuilder<HttpResponseMessage>>> resilienceStrategyNotConfiguredAction =
            builder =>
                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions<HttpResponseMessage>()
                    {
                        FailureRatio = 1
                    });

        isConfigured = ReflectionHelper.CheckRetryStrategyConfigured(resilienceStrategyNotConfiguredAction);
        isConfigured.Should().BeFalse();
    }
}
