using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens the qdrant response status does not indicate success.
/// </summary>
/// <param name="qdrantResponseType">The type of the qdrant response.</param>
/// <param name="status">The status of the qdrant response.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantUnsuccessfulResponseStatusException(Type qdrantResponseType, QdrantStatus status)
	: Exception($"Qdrant response '{qdrantResponseType}' status '{status}' does not indicate success");
