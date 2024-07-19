using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a multivector that consists of multiple dense vectors.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class MultiVector : VectorBase
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override float[] Default =>
        throw new NotSupportedException(
            $"Getting default vector from multivector {GetType()} is not supported since multivector is a multi-component value");

    public override VectorBase this[string vectorName] => throw new NotImplementedException();

    public override VectorBase FirstOrDefault()
    {
        throw new NotImplementedException();
    }

    public override bool ContainsVector(string vectorName)
    {
        throw new NotImplementedException();
    }
}
