using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public abstract class SnapshotTestsBase : QdrantTestsBase
{
    protected async Task AssertSnapshotActualSize(Stream snapshotStream, long expectedSize)
    {
        MemoryStream downloadedSnapshotStream = new MemoryStream();
        await snapshotStream.CopyToAsync(downloadedSnapshotStream);
        downloadedSnapshotStream.Position = 0;

        downloadedSnapshotStream.Length.Should().Be(expectedSize);
    }
}
