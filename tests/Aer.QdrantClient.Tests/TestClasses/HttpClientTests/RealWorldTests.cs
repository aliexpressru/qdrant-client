using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class RealWorldTests
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        _qdrantHttpClient = new QdrantHttpClient(
            new HttpClient()
            {
                BaseAddress = new Uri("http://qdrant1.qdrant.svc.devdb.k8s.ae-rus.net:6333")
            });
    }

    //[Test]
    public async Task TestSearch()
    {
        var vectorRaw = new double[]
        {
            0.06732, -0.053447, 0.018991, 0.091428, 0.065955, -0.083695, -0.001819, -0.025018, -0.034342, 0.064136, -0.060952,
            -0.036844, -0.045032, 0.083695, -0.097796, -0.037754, 0.097341, -0.060042, -0.075507, 0.038891, -0.02479, -0.064136,
            0.090973, -0.016375, 0.008358, 0.093702, 0.080966, -0.04958, -0.083695, 0.094157, 0.015124, -0.043667, -0.006283,
            -0.069594, 0.094157, 0.039118, -0.039346, 0.000174, 0.06732, 0.025472, -0.022061, -0.068685, 0.075507, -0.00021,
            0.015807, -0.023994, 0.008415, -0.002004, 0.045259, -0.030931, -0.004236, 0.035934, -0.030248, -0.096886, -0.026041,
            -0.059132, -0.072778, -0.019218, 0.004094, -0.069594, -0.030476, 0.08415, 0.020014, -0.028315, -0.081876, 0.021833,
            -0.085969, -0.036162, 0.047761, 0.06732, -0.049808, -0.069139, 0.054584, 0.031158, -0.019559, 0.095067, -0.094157,
            -0.021265, 0.029566, 0.060497, -0.0655, -0.009893, -0.008472, -0.015579, 0.075053, 0.002303, 0.010064, -0.061407,
            0.095976, 0.037526, 0.024904, -0.080511, 0.02081, 0.06823, -0.071869, -0.070504, 0.056631, 0.00361, 0.00064, -0.039801
        };

        var vector = vectorRaw.Select(v=>(float) v).ToArray();

        var searchResult = await _qdrantHttpClient.SearchPoints(
            "test_collection_dim_100",
            new SearchPointsRequest(vector, 1)
            {
                WithVector = true,
                WithPayload = true,
            }
            ,
            CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();
    }
}
