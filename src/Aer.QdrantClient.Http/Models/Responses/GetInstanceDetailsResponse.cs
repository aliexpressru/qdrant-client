using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the Qdrant instance details response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetInstanceDetailsResponse
{
	/// <summary>
	/// The qdrant version title.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// The qdrant version.
	/// </summary>
	public string Version { get; set; }

	/// <summary>
	/// The commit hash this version was built from.
	/// </summary>
	public string Commit { get; set; }
}
