using System.Text.Json.Serialization;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Base class for all update collection aliases operations.
/// </summary>
[JsonDerivedType(typeof(CreateAliasOperation))]
[JsonDerivedType(typeof(DeleteAliasOperation))]
[JsonDerivedType(typeof(RenameAliasOperation))]
internal abstract class UpdateCollectionAliasOperationBase
{ }
