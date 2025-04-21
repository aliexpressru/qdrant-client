using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when attempting to create index with additional parameters (such as Tenant or Principal) that restrict certain schemas. 
/// </summary>
/// <param name="unsupportedReason">The reason this schema is not supported.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantUnsupportedFieldSchemaForIndexConfiguration(string unsupportedReason) 
	: Exception($"Payload field schema doses not support selected index configuration. Reason : {unsupportedReason}");
