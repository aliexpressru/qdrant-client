using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Tests.Helpers;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

#if NET9_0_OR_GREATER
internal class QdrantFilterOptimizerTests
{
    [Test]
    public void OptimizeMatchAnyWithOneParameter()
    {
        QdrantFilter filter = Q.MatchAny("whatever", 1);

        var filterString = filter.ToString();

        var expectedOptimizedFilter = """
            {
              "must": [
                {
                  "key": "whatever",
                  "match": {
                    "value": 1
                  }
                }
              ]
            }
            """;

        filter.GetPayloadFieldsWithTypes().Select(ft => ft.Name).Should().BeEquivalentTo(["whatever"]);

        filterString.AssertSameString(expectedOptimizedFilter);
    }
}
#endif
