namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when Qdrant client is not initialized (i.e. its ApiClient property is null).
/// </summary>
public sealed class QdrantClientUninitializedException()
    : Exception($"""
        Qdrant client is not initialized. Either construct Qdrant client via one of the ctor methods that accept {nameof(HttpClient)}
        or http client settings or set {nameof(QdrantHttpClient.ApiClient)} property in a Qdrant client derived from {nameof(QdrantHttpClient)}
        """);
