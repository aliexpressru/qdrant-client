// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverQueried.Global

using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the batch points update request.
/// All operations are executed in the order of their definition.
/// </summary>
public class BatchUpdatePointsRequest
{
    /// <summary>
    /// Points operations to apply.
    /// </summary>
    [JsonInclude]
    internal List<BatchUpdatePointsOperationBase> Operations { get; } = new();

    /// <summary>
    /// Returns the count of operations defined so far on this request.
    /// </summary>
    public int OperationsCount => Operations.Count;

    /// <summary>
    /// Private ctor to enforce fluent interface usage.
    /// </summary>
    private BatchUpdatePointsRequest()
    { }

    /// <summary>
    /// Factory method for fluent interface support.
    /// </summary>
    public static BatchUpdatePointsRequest Create()
    {
        return new BatchUpdatePointsRequest();
    }

    /// <summary>
    /// Append an upsert points operation to batch.
    /// </summary>
    /// <param name="upsertPoints">Points to upsert.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest UpsertPoints<TPayload>(
        IEnumerable<UpsertPointsRequest<TPayload>.UpsertPoint> upsertPoints)
        where TPayload : class
    {
        var opeartion = new UpsertPointsOperation()
        {
            Upsert = new UpsertPointsRequest<TPayload>()
            {
                Points = upsertPoints
            }
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a delete points operation to batch.
    /// </summary>
    /// <param name="pointsToDelete">Points to delete.</param>
    public BatchUpdatePointsRequest DeletePoints(IEnumerable<PointId> pointsToDelete)
    {
        var opeartion = new DeletePointsOperation()
        {
            Delete = new DeletePointsRequest()
            {
                Points = pointsToDelete
            }
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a set points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsToSetPayloadFor">Point ids to set payload for.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest SetPointsPayload<TPayload>(
        TPayload payload,
        IEnumerable<PointId> pointsToSetPayloadFor)
        where TPayload : class
    {
        var opeartion = new SetPointsPayloadOperation()
        {
            SetPayload = new SetPointsPayloadRequest<TPayload>(payload, pointsToSetPayloadFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a set points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsFilterToSetPayloadFor">Points filter to set payload for.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest SetPointsPayload<TPayload>(
        TPayload payload,
        QdrantFilter pointsFilterToSetPayloadFor)
        where TPayload : class
    {
        var opeartion = new SetPointsPayloadOperation()
        {
            SetPayload = new SetPointsPayloadRequest<TPayload>(payload, pointsFilterToSetPayloadFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append an overwrite points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsToOverwritePayloadFor">Point ids to overwrite payload for.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest OverwritePointsPayload<TPayload>(
        TPayload payload,
        IEnumerable<PointId> pointsToOverwritePayloadFor)
        where TPayload : class
    {
        var opeartion = new OverwritePointsPayloadOperation()
        {
            OverwritePayload =
                new OverwritePointsPayloadRequest<TPayload>(payload, pointsToOverwritePayloadFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append an overwrite points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsFilterToOverwritePayloadFor">Points filter to overwrite payload for.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest OverwritePointsPayload<TPayload>(
        TPayload payload,
        QdrantFilter pointsFilterToOverwritePayloadFor)
        where TPayload : class
    {
        var opeartion = new OverwritePointsPayloadOperation()
        {
            OverwritePayload =
                new OverwritePointsPayloadRequest<TPayload>(payload, pointsFilterToOverwritePayloadFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a delete points payload keys operation to batch.
    /// </summary>
    /// <param name="payloadKeysToDelete">Payload keys to delete.</param>
    /// <param name="pointsToDeletePayloadKeysFor">Point ids to delete payload keys for.</param>
    public BatchUpdatePointsRequest DeletePointsPayloadKeys(
        IEnumerable<string> payloadKeysToDelete,
        IEnumerable<PointId> pointsToDeletePayloadKeysFor)
    {
        var opeartion = new DeletePointsPayloadKeysOperation()
        {
            DeletePayload = new DeletePointsPayloadKeysRequest(
                payloadKeysToDelete,
                pointsToDeletePayloadKeysFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a delete points payload keys operation to batch.
    /// </summary>
    /// <param name="payloadKeysToDelete">Payload keys to delete.</param>
    /// <param name="pointsFilterToDeletePayloadKeysFor">Points filter to delete payload for.</param>
    public BatchUpdatePointsRequest DeletePointsPayloadKeys(
        IEnumerable<string> payloadKeysToDelete,
        QdrantFilter pointsFilterToDeletePayloadKeysFor)
    {
        var opeartion = new DeletePointsPayloadKeysOperation()
        {
            DeletePayload = new DeletePointsPayloadKeysRequest(
                payloadKeysToDelete,
                pointsFilterToDeletePayloadKeysFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a clear points payload operation to batch.
    /// </summary>
    /// <param name="pointIdsToClearPayloadFor">Point ids to clear payload for.</param>
    public BatchUpdatePointsRequest ClearPointsPayload(IEnumerable<PointId> pointIdsToClearPayloadFor)
    {
        var opeartion = new ClearPointsPayloadOperation()
        {
            ClearPayload = new ClearPointsPayloadRequest(pointIdsToClearPayloadFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a clear points payload operation to batch.
    /// </summary>
    /// <param name="pointsFilterToClearPayloadFor">Points filter to clear payload for.</param>
    public BatchUpdatePointsRequest ClearPointsPayload(
        QdrantFilter pointsFilterToClearPayloadFor)
    {
        var opeartion = new ClearPointsPayloadOperation()
        {
            ClearPayload = new ClearPointsPayloadRequest(pointsFilterToClearPayloadFor)
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append an update points vectors operation to batch.
    /// </summary>
    /// <param name="updatedPointVectors">Points with updated vectors.</param>
    public BatchUpdatePointsRequest UpdatePointsVectors(PointVector[] updatedPointVectors)
    {
        var opeartion = new UpdatePointsVectorsOperation()
        {
            UpdateVectors = new UpdatePointsVectorsRequest()
            {
                Points = updatedPointVectors
            }
        };

        Operations.Add(opeartion);

        return this;
    }

    /// <summary>
    /// Append a delete points vectors operation to batch.
    /// </summary>
    /// <param name="deletePointsVectorsRequest">Delete points vectors request.</param>
    public BatchUpdatePointsRequest DeletePointsVectors(DeletePointsVectorsRequest deletePointsVectorsRequest)
    {
        var opeartion = new DeletePointsVectorsOperation()
        {
            DeleteVectors = deletePointsVectorsRequest
        };

        Operations.Add(opeartion);

        return this;
    }
}
