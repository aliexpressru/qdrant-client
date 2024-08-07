﻿using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when the attempt is made to get default vector that doesn't exist for a point.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantDefaultVectorNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantDefaultVectorNotFoundException"/> class.
    /// </summary>
    /// <param name="defaultVectorName">The name of the default vector that is not found for point.</param>
    public QdrantDefaultVectorNotFoundException(string defaultVectorName) : base(
        $"Can't find default vector with name {defaultVectorName}")
    { }
}
