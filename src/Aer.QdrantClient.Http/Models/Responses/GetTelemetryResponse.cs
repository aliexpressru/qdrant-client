using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant telemetry collector.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetTelemetryResponse : QdrantResponseBase<JsonObject>
{ }
