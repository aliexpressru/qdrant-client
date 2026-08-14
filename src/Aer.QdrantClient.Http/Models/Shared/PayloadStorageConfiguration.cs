using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The payload storage configuration.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class PayloadStorageConfiguration
{
    /// <summary>
    /// Memory placement of the payload storage:
    /// warmed into a disk cache at startup (cached), or left on disk until first accessed (cold).
    /// Pinned is not supported.
    /// </summary>
    [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<MemoryType>))]
    public MemoryType? Memory { get; set; }
}