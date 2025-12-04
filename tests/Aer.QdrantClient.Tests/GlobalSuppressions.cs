// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style",
    "IDE0058:Expression value is never used",
    Scope = "module",
    Justification = "Using FluentAssertions")]

[assembly: SuppressMessage(
    "CodeQuality",
    "IDE0079:Remove unnecessary suppression",
    Justification = "Using ReSharper suppressions",
    Scope = "module")]

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "Using folders to group types",
    Scope = "namespace",
    Target = "~N:Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots")]

[assembly: SuppressMessage(
    "Structure",
    "NUnit1028:The non-test method is public",
    Justification = "Commented out real-world scenarios",
    Scope = "module")]
