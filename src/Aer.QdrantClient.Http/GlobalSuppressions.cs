// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Abstractions")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Filters.Conditions")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Formulas.Expressions")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Models.Primitives")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Models.Requests.Public")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Models.Responses")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Models.Shared")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Http.Models.Requests.Public.Shared")]

[assembly: SuppressMessage(
    "Design",
    "CA1068:CancellationToken parameters must come last",
    Justification = "Backwards compatibility",
    Scope = "type",
    Target = "~T:Aer.QdrantClient.Http.Abstractions.IQdrantHttpClient")]

[assembly: SuppressMessage(
    "CodeQuality",
    "IDE0079:Remove unnecessary suppression",
    Justification = "Using ReSharper suppressions",
    Scope = "module")]
