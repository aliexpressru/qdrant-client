using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant telemetry collector.
/// </summary>
public sealed class GetTelemetryResponse : QdrantResponseBase<JsonObject>
{ }
