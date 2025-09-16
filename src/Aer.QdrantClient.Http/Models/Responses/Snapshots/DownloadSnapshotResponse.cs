using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the download snapshot operation.
/// </summary>
public sealed class DownloadSnapshotResponse : QdrantResponseBase<DownloadSnapshotResponse.DownloadSnapshotUnit>
{
    // NOTE: this class is manually created unlike any other response classes which are deserialized.
    // Regardless, this class is modelled like all other response classes with Status and Result properties.

    #region Nested classes

    /// <summary>
    /// Represents downloaded snapshot information.
    /// </summary>
    public sealed class DownloadSnapshotUnit
    {
        /// <summary>
        /// The name of the snapshot. Used to name the temporary file on qdrant side.
        /// </summary>
        public string SnapshotName { get; }
        /// <summary>
        /// The stream with binary snapshot data.
        /// </summary>
        public Stream SnapshotDataStream { get; }

        /// <summary>
        /// The snapshot size in bytes.
        /// </summary>
        public long SnapshotSizeBytes { get; }

        /// <summary>
        /// The snapshot size in megabytes.
        /// </summary>
        public double SnapshotSizeMegabytes => SnapshotSizeBytes / 1024.0 / 1024.0;

        /// <summary>
        /// Initializes a new instance of <see cref="DownloadSnapshotUnit"/>
        /// </summary>
        /// <param name="snapshotName">The name of the snapshot file.</param>
        /// <param name="snapshotDataStream">The stream with binary snapshot data.</param>
        /// <param name="snapshotSizeBytes">The snapshot size in bytes.</param>
        public DownloadSnapshotUnit(
            string snapshotName,
            Stream snapshotDataStream,
            long snapshotSizeBytes)
        {
            SnapshotDataStream = snapshotDataStream;
            SnapshotSizeBytes = snapshotSizeBytes;
            SnapshotName = snapshotName;
        }
    }

    #endregion

    /// <summary>
    /// Initializes new instance of <see cref="DownloadSnapshotResponse"/>.
    /// </summary>
    /// <param name="snapshotName">The name of the snapshot.</param>
    /// <param name="snapshotDataStream">The snapshot data stream.</param>
    /// <param name="snapshotSizeBytes">The snapshot length in bytes.</param>
    /// <param name="qdrantOperationStatus">The download snapshot qdrant operation status.</param>
    /// <param name="qdrantOperationTime">The download snapshot time.</param>
    internal DownloadSnapshotResponse(
        string snapshotName,
        Stream snapshotDataStream,
        long snapshotSizeBytes,
        QdrantStatus qdrantOperationStatus,
        TimeSpan qdrantOperationTime)
    {
        Result = new DownloadSnapshotUnit(
            snapshotName,
            snapshotDataStream,
            snapshotSizeBytes
        );

        Status = qdrantOperationStatus;
        
        Time = qdrantOperationTime.TotalSeconds;

        Usage = UsageReport.Empty;
    }
}
