using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Base class for all update collection aliases operations.
/// </summary>
[JsonDerivedType(typeof(CreateAliasOperation))]
[JsonDerivedType(typeof(DeleteAliasOperation))]
[JsonDerivedType(typeof(RenameAliasOperation))]
internal abstract class UpdateCollectionAliasOperationBase
{ }
