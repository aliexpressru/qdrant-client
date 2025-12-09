using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

internal abstract class SnapshotTestsBase : QdrantTestsBase
{
    protected static async Task AssertSnapshotActualSize(Stream snapshotStream, long expectedSize)
    {
        MemoryStream downloadedSnapshotStream = new();
        await snapshotStream.CopyToAsync(downloadedSnapshotStream);
        downloadedSnapshotStream.Position = 0;

        downloadedSnapshotStream.Length.Should().Be(expectedSize);
    }
}
