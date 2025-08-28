using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when the attempt to access or create a collection with invalid name is made.
/// </summary>
/// <param name="invalidCollectionName">The entity name that is deemed invalid.</param>
/// <param name="reason">The reason why the entity name is deemed invalid.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantInvalidEntityNameException(string invalidCollectionName, string reason)
	: Exception($"The qdrant entity name {invalidCollectionName} is invalid. Reason : {reason}");
