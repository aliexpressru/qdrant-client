using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Formulas;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Inference;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.RelevanceFeedback;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Responses.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json.Serialization;
using static Aer.QdrantClient.Http.Models.Requests.CreateFullTextPayloadIndexRequest;
using static Aer.QdrantClient.Http.Models.Requests.CreatePayloadIndexRequest;
using static Aer.QdrantClient.Http.Models.Requests.Public.CreateCollectionRequest;
using static Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.PointsQuery;
using static Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.PointsQuery.DiscoverPointsQuery;
using static Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.PointsQuery.NearestPointsQuery;
using static Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.PointsQuery.RecommendPointsQuery;
using static Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.PointsQuery.RelevanceFeedbackQuery;
using static Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.PointsQuery.RrfQuery;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.OrderByStartFrom;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.PayloadPropertiesSelector;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.QueryVector;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.ReadPointsConsistency;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.SearchVector;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.ShardSelector;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.UpsertPointsBatch;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.VectorSearchParameters;
using static Aer.QdrantClient.Http.Models.Requests.Public.Shared.VectorSelector;
using static Aer.QdrantClient.Http.Models.Requests.Public.UpdateCollectionParametersRequest;
using static Aer.QdrantClient.Http.Models.Requests.Public.UpsertPointsRequest;
using static Aer.QdrantClient.Http.Models.Requests.UpdateCollectionClusteringSetupRequest;
using static Aer.QdrantClient.Http.Models.Requests.UpdateCollectionClusteringSetupRequest.CreateShardingKeyRequest;
using static Aer.QdrantClient.Http.Models.Requests.UpdateCollectionClusteringSetupRequest.DropShardingKeyRequest;
using static Aer.QdrantClient.Http.Models.Requests.UpdateCollectionClusteringSetupRequest.DropShardReplicaRequest;
using static Aer.QdrantClient.Http.Models.Responses.Base.QdrantResponseBase;
using static Aer.QdrantClient.Http.Models.Responses.Base.QdrantResponseBase.UsageReport;
using static Aer.QdrantClient.Http.Models.Responses.Base.QdrantResponseBase.UsageReport.InferenceUsageReport;
using static Aer.QdrantClient.Http.Models.Responses.DropCollectionReplicaFromPeerResponse;
using static Aer.QdrantClient.Http.Models.Responses.GetClusterInfoResponse;
using static Aer.QdrantClient.Http.Models.Responses.GetClusterTelemetryResponse;
using static Aer.QdrantClient.Http.Models.Responses.GetCollectionInfoResponse;
using static Aer.QdrantClient.Http.Models.Responses.GetCollectionInfoResponse.CollectionConfiguration;
using static Aer.QdrantClient.Http.Models.Responses.ReplicateShardsToPeerResponse;
using static Aer.QdrantClient.Http.Models.Shared.FullTextIndexStemmingAlgorithm;
using static Aer.QdrantClient.Http.Models.Shared.QuantizationConfiguration;
using static Aer.QdrantClient.Http.Models.Shared.SparseVectorConfiguration;
using static Aer.QdrantClient.Http.Models.Shared.VectorConfigurationBase;

namespace Aer.QdrantClient.Http.Infrastructure.Json;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    UseStringEnumConverter = true,
    Converters = [
        typeof(CollectionMetadataJsonConverter),
        typeof(FeedbackStrategyJsonConverter),
        typeof(FullTextIndexStemmingAlgorithmJsonConverter),
        typeof(FullTextIndexStopwordsJsonConverter),
        typeof(InferenceObjectJsonConverter),
        typeof(JTokenJsonConverter),
        typeof(ObjectPayloadEnumerableJsonConverter),
        typeof(ObjectPayloadJsonConverter),
        typeof(OrderByStartFromJsonConverter),
        typeof(PayloadJsonConverter),
        typeof(PayloadPropertiesSelectorJsonConverter),
        typeof(PointIdCollectionJsonConverter),
        typeof(PointIdIEnumerableJsonConverter),
        typeof(PointIdJsonConverter),
        typeof(PointIdOrQueryVectorCollectionJsonConverter),
        typeof(PointIdOrQueryVectorJsonConverter),
        typeof(PointsDiscoveryContextCollectionJsonConverter),
        typeof(QdrantCollectionOptimizerStatusJsonConverter),
        typeof(QdrantFilterJsonConverter),
        typeof(QdrantFormulaJsonConverter),
        typeof(QdrantStatusJsonConverter),
        typeof(QuantizationConfigurationJsonConverter),
        typeof(QueryVectorJsonConverter),
        typeof(SearchGroupIdJsonConverter),
        typeof(SearchVectorJsonConverter),
        typeof(ShardKeyJsonConverter),
        typeof(ShardSelectorJsonConverter),
        typeof(VectorConfigurationJsonConverter),
        typeof(VectorJsonConverter),
        typeof(VectorsBatchJsonConverter),
        typeof(VectorSelectorJsonConverter),

        // Enum converters here
        typeof(JsonStringSnakeCaseLowerEnumConverter<FusionAlgorithm>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<RecommendStrategy>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<OrderByDirection>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<PayloadIndexedFieldType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ConsistencyType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<OrderingType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<PointsUpdateMode>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<RecommendStrategy>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<FullTextIndexTokenizerType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<StemmingAlgorithmType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<InferenceObjectKind>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<VectorKind>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<VectorDistanceMetric>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<VectorDataType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ProductQuantizationCompressionRatio>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<BinaryQuantizationEncoding>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<BinaryQuantizationQueryEncoding>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<MultivectorComparator>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ShardingMethod>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<SparseVectorModifier>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<PointsUpdateMode>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<SnapshotPriority>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ShardTransferMethod>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<QdrantOperationStatus>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ConsensusThreadStatus>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<PeerRole>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ReshardingOperationDirection>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<SnapshotType>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<ShardState>),
        typeof(JsonStringSnakeCaseLowerEnumConverter<QdrantCollectionStatus>),
    ],
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.Strict,
    IgnoreReadOnlyProperties = false,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
    IncludeFields = false,
    PropertyNameCaseInsensitive = true,
    UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
)]

// Primitive types

[JsonSerializable(typeof(Uri))]

[JsonSerializable(typeof(VectorBase))]
[JsonSerializable(typeof(DenseVector))]
[JsonSerializable(typeof(NamedVectors))]
[JsonSerializable(typeof(SparseVector))]
[JsonSerializable(typeof(MultiVector))]
[JsonSerializable(typeof(InferredVector))]

[JsonSerializable(typeof(InferenceObject))]
[JsonSerializable(typeof(DocumentInferenceObject))]
[JsonSerializable(typeof(ImageInferenceObject))]
[JsonSerializable(typeof(ObjectInferenceObject))]

[JsonSerializable(typeof(Point))]
[JsonSerializable(typeof(ScoredPoint))]
[JsonSerializable(typeof(PointVector))]

[JsonSerializable(typeof(Payload))]

[JsonSerializable(typeof(PointId))]
[JsonSerializable(typeof(IntegerPointId))]
[JsonSerializable(typeof(GuidPointId))]

[JsonSerializable(typeof(GeoPoint))]

[JsonSerializable(typeof(QueryVector))]
[JsonSerializable(typeof(DenseQueryVector))]
[JsonSerializable(typeof(SparseQueryVector))]
[JsonSerializable(typeof(InferredQueryVector))]

[JsonSerializable(typeof(SearchVector))]
[JsonSerializable(typeof(DenseSearchVector))]
[JsonSerializable(typeof(SparseSearchVector))]
[JsonSerializable(typeof(NamedDenseSearchVector))]
[JsonSerializable(typeof(NamedSparseSearchVector))]

[JsonSerializable(typeof(QdrantFilter))]
[JsonSerializable(typeof(QdrantFormula))]
[JsonSerializable(typeof(PointIdOrQueryVector))]

[JsonSerializable(typeof(ShardKey))]
[JsonSerializable(typeof(IntegerShardKey))]
[JsonSerializable(typeof(StringShardKey))]

[JsonSerializable(typeof(ShardSelector))]
[JsonSerializable(typeof(StringShardKeyShardSelector))]
[JsonSerializable(typeof(IntegerShardKeyShardSelector))]

[JsonSerializable(typeof(VectorSelector))]
[JsonSerializable(typeof(AllVectorsSelector))]
[JsonSerializable(typeof(IncludeNamedVectorsSelector))]

[JsonSerializable(typeof(VectorsLookupLocation))]

[JsonSerializable(typeof(HnswConfiguration))]
[JsonSerializable(typeof(OptimizersConfiguration))]
[JsonSerializable(typeof(SparseVectorConfiguration))]
[JsonSerializable(typeof(StrictModeConfiguration))]
[JsonSerializable(typeof(SparseVectorIndexConfiguration))]

[JsonSerializable(typeof(VectorConfigurationBase))]
[JsonSerializable(typeof(SingleVectorConfiguration))]
[JsonSerializable(typeof(NamedVectorsConfiguration))]
[JsonSerializable(typeof(MultivectorConfiguration))]

[JsonSerializable(typeof(QuantizationConfiguration))]
[JsonSerializable(typeof(ScalarQuantizationConfiguration))]
[JsonSerializable(typeof(ProductQuantizationConfiguration))]
[JsonSerializable(typeof(BinaryQuantizationConfiguration))]

[JsonSerializable(typeof(PayloadPropertiesSelector))]
[JsonSerializable(typeof(AllPayloadPropertiesSelector))]
[JsonSerializable(typeof(IncludePayloadPropertiesSelector))]
[JsonSerializable(typeof(ExcludePayloadPropertiesSelector))]

[JsonSerializable(typeof(ReadPointsConsistency))]
[JsonSerializable(typeof(IntegerReadConsistency))]
[JsonSerializable(typeof(PresetReadConsistency))]

[JsonSerializable(typeof(OrderBySelector))]
[JsonSerializable(typeof(OrderByStartFrom))]
[JsonSerializable(typeof(OrderByStartFromInteger))]
[JsonSerializable(typeof(OrderByStartFromDouble))]
[JsonSerializable(typeof(OrderByStartFromDateTime))]
[JsonSerializable(typeof(CollectionPayloadIndexDefinition))]
[JsonSerializable(typeof(LookupSearchParameters))]

[JsonSerializable(typeof(UpsertPointsBatch))]
[JsonSerializable(typeof(VectorsBatch))]

[JsonSerializable(typeof(PointsDiscoveryContext))]

[JsonSerializable(typeof(VectorSearchParameters))]
[JsonSerializable(typeof(QuantizationParameters))]

[JsonSerializable(typeof(AcornParameters))]

[JsonSerializable(typeof(CollectionMetadata))]

[JsonSerializable(typeof(BatchUpdatePointsOperationBase))]
[JsonSerializable(typeof(ClearPointsPayloadOperation))]
[JsonSerializable(typeof(DeletePointsOperation))]
[JsonSerializable(typeof(DeletePointsPayloadKeysOperation))]
[JsonSerializable(typeof(DeletePointsVectorsOperation))]
[JsonSerializable(typeof(OverwritePointsPayloadOperation))]
[JsonSerializable(typeof(SetPointsPayloadOperation))]
[JsonSerializable(typeof(UpdatePointsVectorsOperation))]
[JsonSerializable(typeof(UpsertPointsOperation))]

[JsonSerializable(typeof(BatchUpdatePointsRequest))]
[JsonSerializable(typeof(DiscoverPointsBatchedRequest))]
[JsonSerializable(typeof(DiscoverPointsRequest))]

[JsonSerializable(typeof(VectorsLookupLocation))]

[JsonSerializable(typeof(Bm25Config))]

[JsonSerializable(typeof(FullTextIndexStemmingAlgorithm))]
[JsonSerializable(typeof(SnowballStemmingAlgorithm))]
[JsonSerializable(typeof(FullTextIndexStopwords))]

[JsonSerializable(typeof(QueryPointsRequest))]

[JsonSerializable(typeof(PointsQuery))]
[JsonSerializable(typeof(NearestPointsQuery))]
[JsonSerializable(typeof(RecommendPointsQuery))]
[JsonSerializable(typeof(DiscoverPointsQuery))]
[JsonSerializable(typeof(ContextQuery))]
[JsonSerializable(typeof(OrderByQuery))]
[JsonSerializable(typeof(FusionQuery))]
[JsonSerializable(typeof(RrfQuery))]
[JsonSerializable(typeof(SampleQuery))]
[JsonSerializable(typeof(FormulaQuery))]
[JsonSerializable(typeof(RelevanceFeedbackQuery))]
[JsonSerializable(typeof(MmrParameters))]
[JsonSerializable(typeof(RecommendPointsQueryUnit))]
[JsonSerializable(typeof(DiscoverPointsQueryUnit))]

[JsonSerializable(typeof(RrfParameters))]
[JsonSerializable(typeof(RelevanceFeedbackQueryUnit))]
[JsonSerializable(typeof(FeedbackStrategy))]
[JsonSerializable(typeof(FeedbackExample))]

[JsonSerializable(typeof(PrefetchPoints))]

// Requests

[JsonSerializable(typeof(QueryPointsBatchedRequest))]
[JsonSerializable(typeof(QueryPointsGroupedRequest))]

[JsonSerializable(typeof(RecommendPointsBatchedRequest))]
[JsonSerializable(typeof(RecommendPointsGroupedRequest))]
[JsonSerializable(typeof(RecommendPointsRequest))]

[JsonSerializable(typeof(SearchPointsBatchedRequest))]
[JsonSerializable(typeof(SearchPointsDistanceMatrixRequest))]
[JsonSerializable(typeof(SearchPointsGroupedRequest))]
[JsonSerializable(typeof(SearchPointsRequest))]

[JsonSerializable(typeof(UpdateCollectionAliasesRequest))]
[JsonSerializable(typeof(UpdateCollectionAliasOperationBase))]
[JsonSerializable(typeof(CreateAliasOperation))]
[JsonSerializable(typeof(DeleteAliasOperation))]
[JsonSerializable(typeof(RenameAliasOperation))]

[JsonSerializable(typeof(ClearPointsPayloadRequest))]
[JsonSerializable(typeof(CountPointsRequest))]

[JsonSerializable(typeof(CreateCollectionRequest))]
[JsonSerializable(typeof(InitFromCollection))]

[JsonSerializable(typeof(DeletePointsPayloadKeysRequest))]
[JsonSerializable(typeof(DeletePointsVectorsRequest))]
[JsonSerializable(typeof(FacetCountPointsRequest))]
[JsonSerializable(typeof(OverwritePointsPayloadRequest))]
[JsonSerializable(typeof(SetPointsPayloadRequest))]
[JsonSerializable(typeof(UpdateCollectionParametersRequest))]
[JsonSerializable(typeof(CollectionParameters))]
[JsonSerializable(typeof(UpdatePointsVectorsRequest))]
[JsonSerializable(typeof(UpsertPointsRequest))]
[JsonSerializable(typeof(UpsertPoint))]

[JsonSerializable(typeof(CreateFullTextPayloadIndexRequest))]
[JsonSerializable(typeof(FullTextPayloadFieldSchema))]

[JsonSerializable(typeof(CreatePayloadIndexRequest))]
[JsonSerializable(typeof(FieldSchemaUnit))]
[JsonSerializable(typeof(CreateShardKeyRequest))]
[JsonSerializable(typeof(DeletePointsRequest))]
[JsonSerializable(typeof(DeleteShardKeyRequest))]
[JsonSerializable(typeof(EmptyRequest))]
[JsonSerializable(typeof(GetPointsRequest))]
[JsonSerializable(typeof(RecoverEntityFromSnapshotRequest))]
[JsonSerializable(typeof(ScrollPointsRequest))]
[JsonSerializable(typeof(SetLockOptionsRequest))]

[JsonSerializable(typeof(UpdateCollectionClusteringSetupRequest))]
[JsonSerializable(typeof(ShardOperationDescription))]
[JsonSerializable(typeof(ReplicatePointsOperationDescription))]
[JsonSerializable(typeof(ReshardingOperationDescription))]
[JsonSerializable(typeof(DropShardReplicaDescriptor))]
[JsonSerializable(typeof(ShardKeyOperationDescription))]
[JsonSerializable(typeof(DropShardKeyOperationDescription))]
[JsonSerializable(typeof(CreateShardingKeyRequest))]
[JsonSerializable(typeof(MoveShardRequest))]
[JsonSerializable(typeof(ReplicateShardRequest))]
[JsonSerializable(typeof(ReplicatePointsRequest))]
[JsonSerializable(typeof(AbortShardTransferRequest))]
[JsonSerializable(typeof(RestartShardTransferRequest))]
[JsonSerializable(typeof(DropShardReplicaRequest))]
[JsonSerializable(typeof(StartReshardingOperationRequest))]
[JsonSerializable(typeof(AbortReshardingOperationRequest))]
[JsonSerializable(typeof(DropShardingKeyRequest))]

// Responses

[JsonSerializable(typeof(QdrantOperationResult))]
[JsonSerializable(typeof(QdrantResponseBase))]
[JsonSerializable(typeof(UsageReport))]
[JsonSerializable(typeof(HardwareUsageReport))]
[JsonSerializable(typeof(InferenceUsageReport))]
[JsonSerializable(typeof(ModelUsage))]

[JsonSerializable(typeof(CheckIsPeerEmptyResponse))]
[JsonSerializable(typeof(QdrantResponseBase<bool>))]
[JsonSerializable(typeof(ClearPeerResponse))]
[JsonSerializable(typeof(DrainPeerResponse))]
[JsonSerializable(typeof(DropCollectionReplicaFromPeerResponse))]
[JsonSerializable(typeof(QdrantResponseBase<DropCollectionReplicaFromPeerResponseUnit>))]
[JsonSerializable(typeof(DropCollectionReplicaFromPeerResponseUnit))]

[JsonSerializable(typeof(GetPeerResponse))]
[JsonSerializable(typeof(QdrantResponseBase<GetPeerResponse.PeerInfo>))]
[JsonSerializable(typeof(GetPeerResponse.PeerInfo))]

[JsonSerializable(typeof(ListCollectionInfoResponse))]
[JsonSerializable(typeof(QdrantResponseBase<Dictionary<string, GetCollectionInfoResponse.CollectionInfo>>))]

[JsonSerializable(typeof(ReplicateShardsToPeerResponse))]
[JsonSerializable(typeof(QdrantResponseBase<ReplicateShardsToPeerResponseUnit>))]
[JsonSerializable(typeof(ReplicateShardsToPeerResponseUnit))]

[JsonSerializable(typeof(RestoreShardReplicationFactorResponse))]

[JsonSerializable(typeof(ConsensusThreadState))]
[JsonSerializable(typeof(ReshardingOperationInfo))]
[JsonSerializable(typeof(ShardTransferInfo))]

[JsonSerializable(typeof(CreateSnapshotResponse))]
[JsonSerializable(typeof(QdrantResponseBase<SnapshotInfo>))]
[JsonSerializable(typeof(SnapshotInfo))]

[JsonSerializable(typeof(DownloadSnapshotResponse))]
[JsonSerializable(typeof(QdrantResponseBase<DownloadSnapshotResponse.DownloadSnapshotUnit>))]
[JsonSerializable(typeof(DownloadSnapshotResponse.DownloadSnapshotUnit))]
[JsonSerializable(typeof(ListSnapshotsResponse))]
[JsonSerializable(typeof(BatchPointsOperationResponse))]
[JsonSerializable(typeof(QdrantResponseBase<QdrantOperationResult[]>))]

[JsonSerializable(typeof(CheckCollectionExistsResponse))]
[JsonSerializable(typeof(QdrantResponseBase<CheckCollectionExistsResponse.CollectionExistenceResult>))]
[JsonSerializable(typeof(CheckCollectionExistsResponse.CollectionExistenceResult))]

[JsonSerializable(typeof(ClearReportedIssuesResponse))]
[JsonSerializable(typeof(CountPointsResponse))]
[JsonSerializable(typeof(CountPointsResponse.CountPointsResult))]
[JsonSerializable(typeof(DefaultOperationResponse))]

[JsonSerializable(typeof(FacetCountPointsResponse))]
[JsonSerializable(typeof(QdrantResponseBase<FacetCountPointsResponse.FacetCountHitsUnit>))]
[JsonSerializable(typeof(FacetCountPointsResponse.FacetCountHitsUnit))]

[JsonSerializable(typeof(GetClusterInfoResponse))]
[JsonSerializable(typeof(QdrantResponseBase<GetClusterInfoResponse.ClusterInfo>))]
[JsonSerializable(typeof(GetClusterInfoResponse.ClusterInfo))]
[JsonSerializable(typeof(PeerInfoUint))]
[JsonSerializable(typeof(RaftInfoUnit))]
[JsonSerializable(typeof(ConsensusThreadState))]
[JsonSerializable(typeof(MessageSendFailureUnit))]

[JsonSerializable(typeof(GetClusterTelemetryResponse))]
[JsonSerializable(typeof(QdrantResponseBase<ClusterTelemetryInfo>))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.CollectionTelemetry))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.CollectionTelemetry.ShardInfo))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.CollectionTelemetry.ShardInfo.ReplicaInfo))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.CollectionTelemetry.ShardInfo.ReplicaInfo.PartialSnapshotInfo))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.ClusterTelemetry))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.ClusterTelemetry.PeerInfo))]
[JsonSerializable(typeof(GetClusterTelemetryResponse.ClusterTelemetry.PeerInfo.DistributedPeerDetails))]

[JsonSerializable(typeof(GetCollectionClusteringInfoResponse))]
[JsonSerializable(typeof(QdrantResponseBase<GetCollectionClusteringInfoResponse.CollectionClusteringInfo>))]
[JsonSerializable(typeof(QdrantResponseBase<GetCollectionClusteringInfoResponse.LocalShardInfo>))]
[JsonSerializable(typeof(QdrantResponseBase<GetCollectionClusteringInfoResponse.RemoteShardInfo>))]
[JsonSerializable(typeof(GetCollectionClusteringInfoResponse.CollectionClusteringInfo))]

[JsonSerializable(typeof(GetCollectionInfoResponse))]
[JsonSerializable(typeof(QdrantResponseBase<GetCollectionInfoResponse.CollectionInfo>))]
[JsonSerializable(typeof(GetCollectionInfoResponse.CollectionInfo))]
[JsonSerializable(typeof(QdrantOptimizerStatusUint))]
[JsonSerializable(typeof(CollectionConfiguration))]
[JsonSerializable(typeof(PayloadSchemaPropertyDefinition))]
[JsonSerializable(typeof(PayloadSchemaPropertyDefinition.PayloadSchemaPropertyParameters))]
[JsonSerializable(typeof(WalConfiguration))]

[JsonSerializable(typeof(GetCollectionOptimizationProgressResponse))]

internal partial class SourceGenerationContext : JsonSerializerContext { }
