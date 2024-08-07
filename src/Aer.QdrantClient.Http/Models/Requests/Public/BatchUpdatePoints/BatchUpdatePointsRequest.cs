﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the batch points update request.
/// All operations are executed in the order of their definition.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class BatchUpdatePointsRequest
{
    /// <summary>
    /// Points operations to apply.
    /// </summary>
    [JsonInclude]
    internal List<BatchUpdatePointsOperationBase> Operations { get; } = [];

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
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest UpsertPoints<TPayload>(
        IEnumerable<UpsertPointsRequest<TPayload>.UpsertPoint> upsertPoints,
        ShardSelector shardSelector = null)
        where TPayload : class
    {
        var operation = new UpsertPointsOperation()
        {
            Upsert = new UpsertPointsRequest<TPayload>()
            {
                Points = upsertPoints,
                ShardKey = shardSelector,
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a delete points operation to batch.
    /// </summary>
    /// <param name="pointsToDelete">Points to delete.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest DeletePoints(
        IEnumerable<PointId> pointsToDelete,
        ShardSelector shardSelector = null)
    {
        var operation = new DeletePointsOperation()
        {
            Delete = new DeletePointsRequest()
            {
                Points = pointsToDelete,
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a set points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsToSetPayloadFor">Point ids to set payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest SetPointsPayload<TPayload>(
        TPayload payload,
        IEnumerable<PointId> pointsToSetPayloadFor,
        ShardSelector shardSelector = null)
        where TPayload : class
    {
        var operation = new SetPointsPayloadOperation()
        {
            SetPayload = new SetPointsPayloadRequest<TPayload>(payload, pointsToSetPayloadFor)
            {
                ShardKey = shardSelector,
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a set points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsFilterToSetPayloadFor">Points filter to set payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest SetPointsPayload<TPayload>(
        TPayload payload,
        QdrantFilter pointsFilterToSetPayloadFor,
        ShardSelector shardSelector = null)
        where TPayload : class
    {
        var operation = new SetPointsPayloadOperation()
        {
            SetPayload = new SetPointsPayloadRequest<TPayload>(payload, pointsFilterToSetPayloadFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append an overwrite points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsToOverwritePayloadFor">Point ids to overwrite payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest OverwritePointsPayload<TPayload>(
        TPayload payload,
        IEnumerable<PointId> pointsToOverwritePayloadFor,
        ShardSelector shardSelector = null)
        where TPayload : class
    {
        var operation = new OverwritePointsPayloadOperation()
        {
            OverwritePayload =
                new OverwritePointsPayloadRequest<TPayload>(payload, pointsToOverwritePayloadFor)
                {
                    ShardKey = shardSelector
                }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append an overwrite points payload operation to batch.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsFilterToOverwritePayloadFor">Points filter to overwrite payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public BatchUpdatePointsRequest OverwritePointsPayload<TPayload>(
        TPayload payload,
        QdrantFilter pointsFilterToOverwritePayloadFor,
        ShardSelector shardSelector = null)
        where TPayload : class
    {
        var operation = new OverwritePointsPayloadOperation()
        {
            OverwritePayload =
                new OverwritePointsPayloadRequest<TPayload>(payload, pointsFilterToOverwritePayloadFor)
                {
                    ShardKey = shardSelector
                }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a delete points payload keys operation to batch.
    /// </summary>
    /// <param name="payloadKeysToDelete">Payload keys to delete.</param>
    /// <param name="pointsToDeletePayloadKeysFor">Point ids to delete payload keys for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest DeletePointsPayloadKeys(
        IEnumerable<string> payloadKeysToDelete,
        IEnumerable<PointId> pointsToDeletePayloadKeysFor,
        ShardSelector shardSelector = null)
    {
        var operation = new DeletePointsPayloadKeysOperation()
        {
            DeletePayload = new DeletePointsPayloadKeysRequest(
                payloadKeysToDelete,
                pointsToDeletePayloadKeysFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a delete points payload keys operation to batch.
    /// </summary>
    /// <param name="payloadKeysToDelete">Payload keys to delete.</param>
    /// <param name="pointsFilterToDeletePayloadKeysFor">Points filter to delete payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest DeletePointsPayloadKeys(
        IEnumerable<string> payloadKeysToDelete,
        QdrantFilter pointsFilterToDeletePayloadKeysFor,
        ShardSelector shardSelector = null)
    {
        var operation = new DeletePointsPayloadKeysOperation()
        {
            DeletePayload = new DeletePointsPayloadKeysRequest(
                payloadKeysToDelete,
                pointsFilterToDeletePayloadKeysFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a clear points payload operation to batch.
    /// </summary>
    /// <param name="pointIdsToClearPayloadFor">Point ids to clear payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest ClearPointsPayload(
        IEnumerable<PointId> pointIdsToClearPayloadFor,

        ShardSelector shardSelector = null)
    {
        var operation = new ClearPointsPayloadOperation()
        {
            ClearPayload = new ClearPointsPayloadRequest(pointIdsToClearPayloadFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a clear points payload operation to batch.
    /// </summary>
    /// <param name="pointsFilterToClearPayloadFor">Points filter to clear payload for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest ClearPointsPayload(
        QdrantFilter pointsFilterToClearPayloadFor,
        ShardSelector shardSelector = null)
    {
        var operation = new ClearPointsPayloadOperation()
        {
            ClearPayload = new ClearPointsPayloadRequest(pointsFilterToClearPayloadFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append an update points vectors operation to batch.
    /// </summary>
    /// <param name="updatedPointVectors">Points with updated vectors.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest UpdatePointsVectors(
        PointVector[] updatedPointVectors,
        ShardSelector shardSelector = null)
    {
        var operation = new UpdatePointsVectorsOperation()
        {
            UpdateVectors = new UpdatePointsVectorsRequest()
            {
                Points = updatedPointVectors,
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a delete points vectors operation to batch.
    /// </summary>
    /// <param name="vectorNamesToDelete">Names of vectors to delete.</param>
    /// <param name="pointsToDeleteVectorsFor">Point ids to delete vectors for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest DeletePointsVectors(
        IEnumerable<string> vectorNamesToDelete,
        IEnumerable<PointId> pointsToDeleteVectorsFor,
        ShardSelector shardSelector = null)
    {
        var operation = new DeletePointsVectorsOperation()
        {
            DeleteVectors = new DeletePointsVectorsRequest(vectorNamesToDelete, pointsToDeleteVectorsFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }

    /// <summary>
    /// Append a delete points vectors operation to batch.
    /// </summary>
    /// <param name="vectorNamesToDelete">Names of vectors to delete.</param>>
    /// <param name="pointsFilterToDeleteVectorsFor">Points filter to delete vectors for.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public BatchUpdatePointsRequest DeletePointsVectors(
        IEnumerable<string> vectorNamesToDelete,
        QdrantFilter pointsFilterToDeleteVectorsFor,
        ShardSelector shardSelector = null)
    {
        var operation = new DeletePointsVectorsOperation()
        {
            DeleteVectors = new DeletePointsVectorsRequest(vectorNamesToDelete, pointsFilterToDeleteVectorsFor)
            {
                ShardKey = shardSelector
            }
        };

        Operations.Add(operation);

        return this;
    }
}
