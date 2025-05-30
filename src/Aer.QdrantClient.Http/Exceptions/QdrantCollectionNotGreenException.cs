﻿using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when waiting time for the collection to become green exceeded the specified timeout value.
/// </summary>
/// <param name="collectionName">The collection to wait to become green.</param>
/// <param name="waitForCollectionGreenTimeout">The time to wait for collection to become green.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantCollectionNotGreenException(string collectionName, TimeSpan waitForCollectionGreenTimeout) 
	: Exception($"The collection {collectionName} is not {QdrantCollectionStatus.Green} for the timeout duration {waitForCollectionGreenTimeout:g}");