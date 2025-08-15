using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the Qdrant instance details response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetInstanceDetailsResponse
{
    private Version _parsedVersion;
    
	/// <summary>
	/// The qdrant version title.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// The qdrant version.
	/// </summary>
	public string Version { get; set; }
    
    /// <summary>
    /// The <see cref="System.Version"/> parsed from the <see cref="Version"/> string.
    /// </summary>
    public Version ParsedVersion
    {
        get
        {
            if (_parsedVersion == null)
            {
                _parsedVersion = System.Version.Parse(Version);
            }
            return _parsedVersion;
        }
    }

	/// <summary>
	/// The commit hash this version was built from.
	/// </summary>
	public string Commit { get; set; }
}
