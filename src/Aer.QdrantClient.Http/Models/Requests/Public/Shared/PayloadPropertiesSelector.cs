using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a returned point payload selector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class PayloadPropertiesSelector
{
    internal sealed class AllPayloadPropertiesSelector(bool areAllPayloadPropertiesSelected) : PayloadPropertiesSelector
    {
        public bool AreAllPayloadPropertiesSelected { get; } = areAllPayloadPropertiesSelected;
    }

    internal sealed class IncludePayloadPropertiesSelector(params string[] includedPayloadProperties) : PayloadPropertiesSelector
    {
        public string[] IncludedPayloadProperties { get; } = includedPayloadProperties;
    }

    internal sealed class ExcludePayloadPropertiesSelector(string[] excludedPayloadProperties) : PayloadPropertiesSelector
    {
        public string[] ExcludedPayloadProperties { get; } = excludedPayloadProperties;
    }

    /// <summary>
    /// Creates an instance of the payload properties selector that selects all payload properties to be returned.
    /// </summary>
    public static PayloadPropertiesSelector All { get; } = new AllPayloadPropertiesSelector(true);

    /// <summary>
    /// Creates an instance of the payload properties selector that selects none of the payload properties to be returned.
    /// </summary>
    public static PayloadPropertiesSelector None { get; } = new AllPayloadPropertiesSelector(false);

    /// <summary>
    /// Creates an instance of the payload properties selector that selects only specified payload properties to be returned.
    /// </summary>
    public static PayloadPropertiesSelector Include(params string[] includedPayloadProperties) =>
        new IncludePayloadPropertiesSelector(includedPayloadProperties);

    /// <summary>
    /// Creates an instance of the payload properties selector that selects all payload properties except specified to be returned.
    /// </summary>
    public static PayloadPropertiesSelector Exclude(params string[] excludedPayloadProperties) =>
        new ExcludePayloadPropertiesSelector(excludedPayloadProperties);

    /// <summary>
    /// Implicitly converts boolean values to <see cref="AllPayloadPropertiesSelector"/> instances.
    /// </summary>
    /// <param name="value">If <c>true</c> - includes all payload properties to the result.
    /// If <c>false</c> excludes all payload properties.</param>
    public static implicit operator PayloadPropertiesSelector(bool value) => new AllPayloadPropertiesSelector(value);

    /// <summary>
    /// Implicitly converts string array values to <see cref="AllPayloadPropertiesSelector"/> instances.
    /// </summary>
    /// <param name="includedPayloadProperties">If not null or empty - includes all specified payload properties to the result.
    /// If null or empty - includes all payload properties.</param>
    public static implicit operator PayloadPropertiesSelector(string[] includedPayloadProperties)
    {
        if (includedPayloadProperties is null or { Length: 0 })
        {
            return new AllPayloadPropertiesSelector(true);
        }

        return new IncludePayloadPropertiesSelector(includedPayloadProperties);
    }
}
