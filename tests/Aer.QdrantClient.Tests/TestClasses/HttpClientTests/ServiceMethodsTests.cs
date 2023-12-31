﻿using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class ServiceMethodsTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage();
    }

    [Test]
    public async Task TestGetTelemetryData_NotAnonymized()
    {
        var telemetry = await _qdrantHttpClient.GetTelemetry(
            CancellationToken.None,
            detailsLevel: 3,
            isAnonymizeTelemetryData: false);

        telemetry.Status.IsSuccess.Should().BeTrue();
        telemetry.Result.Should().NotBeNull();
    }

    [Test]
    public async Task TestGetTelemetryData_Anonymized()
    {
        var telemetry = await _qdrantHttpClient.GetTelemetry(
            CancellationToken.None,
            detailsLevel: 3,
            isAnonymizeTelemetryData: true);

        telemetry.Status.IsSuccess.Should().BeTrue();
        telemetry.Result.Should().NotBeNull();
    }

    [Test]
    public async Task TestCollectionReady_CollectionDoesNotExist()
    {
        var act = () => _qdrantHttpClient.EnsureCollectionReady(
                TestCollectionName,
                CancellationToken.None);

        await act.Should().ThrowAsync<QdrantUnsuccessfullResponseStatusException>()
            .Where(e => e.Message.Contains("not found", StringComparison.InvariantCultureIgnoreCase));
    }

    [Test]
    public async Task TestCollectionReady_InvalidTimeout()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var act = () => _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            pollingInterval: TimeSpan.FromMinutes(1),
            timeout: TimeSpan.FromSeconds(1));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(e => e.Message.Contains("should be greater than"));
    }

    [Test]
    public async Task TestCollectionReady_OneSuccessfullResponse()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var act = () => _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            pollingInterval: TimeSpan.FromMilliseconds(100),
            timeout: TimeSpan.FromSeconds(30),
            requiredNumberOfGreenCollectionResponses: 1); // default value

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task TestCollectionReady_SeveralSuccessfullResponses()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var act = () => _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            pollingInterval: TimeSpan.FromMilliseconds(100),
            timeout: TimeSpan.FromSeconds(30),
            requiredNumberOfGreenCollectionResponses: 3);

        await act.Should().NotThrowAsync();
    }
}
