using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a named vectors collection.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class NamedVectors : VectorBase
{
    /// <summary>
    /// The name to vector mapping.
    /// </summary>
    public required Dictionary<string, VectorBase> Vectors { init; get; } = new();

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Named;

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorBase Default
    {
        get
        {
            EnsureNotEmpty();

            if (Vectors.TryGetValue(DefaultVectorName, out VectorBase defaultVector))
            {
                return defaultVector;
            }

            throw new QdrantDefaultVectorNotFoundException(DefaultVectorName);
        }
    }

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault()
    {
        EnsureNotEmpty();

        return Vectors.First().Value;
    }

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
    {
        EnsureNotEmpty();

        return Vectors.ContainsKey(vectorName);
    }

    /// <inheritdoc/>
    public override VectorBase this[string vectorName]
    {
        get
        {
            EnsureNotEmpty();

            if (Vectors.TryGetValue(vectorName, out var vector))
            {
                return vector;
            }

            throw new KeyNotFoundException($"Named vector {vectorName} for point is not found");
        }
    }

    private void EnsureNotEmpty()
    {
        if (Vectors.Count is 0)
        {
            throw new InvalidOperationException("Named vectors collection for point is empty");
        }
    }
}
