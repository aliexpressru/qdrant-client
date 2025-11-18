using System.Net;

namespace Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;

#if NETSTANDARD2_0 || NETSTANDARD2_1

internal static class HttpResponseMessageExtensions
{
	private const string StatusCodeKeyName = "StatusCode";

	extension(HttpRequestException httpRequestException)
    {
        public bool SetStatusCode(HttpStatusCode httpStatusCode)
        {
            httpRequestException.Data[StatusCodeKeyName] = httpStatusCode;

            return false;
        }

        public HttpStatusCode? GetStatusCode()
        {
            return (HttpStatusCode?) httpRequestException.Data[StatusCodeKeyName];
        }
    }

    extension(HttpResponseMessage httpResponseMessage)
    {
        public HttpResponseMessage SetStatusCode()
        {
            try
            {
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when (ex.SetStatusCode(httpResponseMessage.StatusCode))
            {
                // Intentionally left empty. Will never be reached.
            }

            return httpResponseMessage;
        }
    }
	
	extension(HttpContent httpContent)
    {
        public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken)
        {
            return await httpContent.ReadAsStreamAsync().WaitAsync(
                timeout: Timeout.InfiniteTimeSpan,
                TimeProvider.System,
                cancellationToken: cancellationToken);
        }

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken)
        {
            return await httpContent.ReadAsStringAsync().WaitAsync(
                timeout: Timeout.InfiniteTimeSpan,
                TimeProvider.System,
                cancellationToken: cancellationToken);
        }
    }
}

#endif

#if NETSTANDARD2_0

internal static class Netstandard20Extensions
{
	extension<T>(IEnumerable<T> collection)
    {
        public HashSet<T> ToHashSet() => [..collection];
    }

	extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        public bool TryAdd(TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);
            return true;
        }
    }

	extension<TKey, TValue>(KeyValuePair<TKey, TValue> target)
    {
        public void Deconstruct(
            out TKey key,
            out TValue value)
        {
            key = target.Key;
            value = target.Value;
        }
    }

	extension(string target)
    {
        public bool Contains(string value, StringComparison comparisonType)
        {
            return target.IndexOf(value, comparisonType) >= 0;
        }
    }
}

#endif