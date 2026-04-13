using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Diagnostics;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class TracingTests : QdrantTestsBase
{
    private readonly ConcurrentBag<Activity> _capturedActivities = [];
    private ActivityListener _activityListener;

    [SetUp]
    public async Task Setup()
    {
        Initialize();
        await ResetStorage();
    }

    [TearDown]
    public void Cleanup()
    {
        _activityListener?.Dispose();
        _capturedActivities.Clear();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CreateCollection_WithTracingDisabled_DoesNotCreateSpans(bool createClientViaFactory)
    {
        var client = createClientViaFactory ? CreateClientByFactory(disableTracing: true) : CreateClient(disableTracing: true);

        using var activity = new Activity("test-parent").Start();

        (
            await client.CreateCollection(
            TestCollectionName,
            new(VectorDistanceMetric.Dot, vectorSize: 10, isServeVectorsFromDisk: true),
            CancellationToken.None)
        ).EnsureSuccess();

        _capturedActivities.Should().BeEmpty();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CreateCollection_WithTracingEnabled_CreatesSpans(bool createClientViaFactory)
    {
        var client = createClientViaFactory ? CreateClientByFactory(disableTracing: false) : CreateClient(disableTracing: false);

        using var activity = new Activity("test-parent").Start();

        (
            await client.CreateCollection(
            TestCollectionName,
            new(VectorDistanceMetric.Dot, vectorSize: 10, isServeVectorsFromDisk: true),
            CancellationToken.None)
        ).EnsureSuccess();

        _capturedActivities.Should().NotBeEmpty();
        _capturedActivities.Should().Contain(a => a.DisplayName.Contains("qdrant.http"));
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task MultipleCompoundOperations_WithTracingEnabled_CreatesSpans(bool createClientViaFactory)
    {
        var client = createClientViaFactory ? CreateClientByFactory(disableTracing: false) : CreateClient(disableTracing: false);

        using var activity = new Activity("test-parent").Start();

        await PrepareCollection(client, TestCollectionName, vectorCount: 100);
        await PrepareCollection(client, TestCollectionName2, vectorCount: 100);

        var listCollectionInfos = client.ListCollectionInfo(isCountExactPointsNumber: true, CancellationToken.None);

        _capturedActivities.Should().NotBeEmpty();
        _capturedActivities.Should().Contain(a => a.DisplayName.Contains("qdrant.http"));
    }

    private QdrantHttpClient CreateClient(bool disableMetrics = false, bool disableTracing = false)
    {
        var defaultClientConfig = GetClientSettings(disableMetrics, disableTracing);

        ServiceCollection services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        services.AddSingleton(configuration);

        services.AddQdrantClientFactory();
        AddTestLogger(services);

        services.AddQdrantHttpClient(
                configureQdrantClientSettings: config =>
                {
                    config.HttpAddress = defaultClientConfig.HttpAddress;
                    config.ApiKey = defaultClientConfig.ApiKey;
                    config.HttpClientTimeout = defaultClientConfig.HttpClientTimeout;
                    config.DisableMetrics = defaultClientConfig.DisableMetrics;
                    config.DisableTracing = defaultClientConfig.DisableTracing;
                }
            );

        AddTestActivityListener(disableTracing);

        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetRequiredService<QdrantHttpClient>();

        return client;
    }

    private IQdrantHttpClient CreateClientByFactory(bool disableMetrics = false, bool disableTracing = false)
    {
        var defaultClientConfig = GetClientSettings(disableMetrics, disableTracing);

        ServiceCollection services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        services.AddSingleton(configuration);

        services.AddQdrantClientFactory();
        AddTestLogger(services);

        AddTestActivityListener(disableTracing);

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IQdrantClientFactory>();

        var clientName = "Client1";

        clientFactory.AddClientConfiguration(clientName, defaultClientConfig);

        var client = clientFactory.CreateClient(clientName);

        return client;
    }

    private static QdrantClientSettings GetClientSettings(bool disableMetrics = false, bool disableTracing = false)
    {
        var defaultClientConfig = new QdrantClientSettings()
        {
            HttpAddress = "http://localhost:6333",
            ApiKey = "test",
            HttpClientTimeout = TimeSpan.FromMinutes(10),
            DisableMetrics = disableMetrics,
            DisableTracing = disableTracing
        };

        return defaultClientConfig;
    }

    private void AddTestActivityListener(bool disableTracing)
    {
        if (disableTracing)
        {
            return;
        }

        var activityServiceName = QdrantHttpClientDiagnosticConstants.TracingActivityServiceName;

        // Here source name will be the actual name of this service which is Aer.QdrantClient.Tests
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == activityServiceName,
            Sample = (ref options) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                if (activity.Source.Name == activityServiceName)
                {
                    _capturedActivities.Add(activity);
                }
            }
        };

        ActivitySource.AddActivityListener(_activityListener);
    }
}

