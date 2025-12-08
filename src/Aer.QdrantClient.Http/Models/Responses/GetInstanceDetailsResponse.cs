using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the Qdrant instance details response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class GetInstanceDetailsResponse
{
    /// <summary>
    /// The qdrant version title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// The qdrant version.
    /// </summary>
    public string Version { get; init; }

    /// <summary>
    /// The <see cref="System.Version"/> parsed from the <see cref="Version"/> string.
    /// </summary>
    public Version ParsedVersion
    {
        get {
            if (field == null)
            {
                field = System.Version.Parse(Version);
            }

            return field;
        }
    }

    /// <summary>
    /// The commit hash this version was built from.
    /// </summary>
    public string Commit { get; init; }
}
