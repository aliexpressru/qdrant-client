using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents one bulk update points operation base class. Used to group operations in one collection.
/// </summary>
[JsonDerivedType(typeof(ClearPointsPayloadOperation))]
[JsonDerivedType(typeof(DeletePointsOperation))]
[JsonDerivedType(typeof(DeletePointsPayloadKeysOperation))]
[JsonDerivedType(typeof(DeletePointsVectorsOperation))]
[JsonDerivedType(typeof(OverwritePointsPayloadOperation))]
[JsonDerivedType(typeof(SetPointsPayloadOperation))]
[JsonDerivedType(typeof(UpdatePointsVectorsOperation))]
[JsonDerivedType(typeof(UpsertPointsOperation))]
[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
internal abstract class BatchUpdatePointsOperationBase
{ }
