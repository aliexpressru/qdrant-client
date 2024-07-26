using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a returned point payload selector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class PayloadPropertiesSelector
{
    internal class AllPayloadPropertiesSelector : PayloadPropertiesSelector
    {
        public bool AreAllPayloadPropertiesSelected { get; }

        public AllPayloadPropertiesSelector(bool areAllPayloadPropertiesSelected)
        {
            AreAllPayloadPropertiesSelected = areAllPayloadPropertiesSelected;
        }
    }

    internal class IncludePayloadPropertiesSelector : PayloadPropertiesSelector
    {
        public string[] IncludedPayloadProperties { get; }

        public IncludePayloadPropertiesSelector(params string[] includedPayloadProperties)
        {
            IncludedPayloadProperties = includedPayloadProperties;
        }
    }

    internal class ExcludePayloadPropertiesSelector : PayloadPropertiesSelector
    {
        public string[] ExcludedPayloadProperties { get; }

        public ExcludePayloadPropertiesSelector(string[] excludedPayloadProperties)
        {
            ExcludedPayloadProperties = excludedPayloadProperties;
        }
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

    #region Operators

    /// <summary>
    /// Implicitly converts boolean values to <see cref="AllPayloadPropertiesSelector"/> instances.
    /// </summary>
    /// <param name="value">If <c>true</c> - includes all payload properties to the result.
    /// If <c>false</c> excludes all payload properties.</param>
    public static implicit operator PayloadPropertiesSelector(bool value)
    {
        return new AllPayloadPropertiesSelector(value);
    }

    #endregion
}
