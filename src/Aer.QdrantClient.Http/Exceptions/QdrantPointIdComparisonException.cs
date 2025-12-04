using Aer.QdrantClient.Http.Models.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to compare two incomparable PointIds of different types.
/// </summary>
/// <param name="leftPointId">The point id to compare.</param>
/// <param name="rightPointId">The point id to compare to.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantPointIdComparisonException(PointId leftPointId, PointId rightPointId)
    : Exception($"Can't compare point id {leftPointId.ToString(includeTypeInfo: true)} to point id {rightPointId}");
