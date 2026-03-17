using System.Net;

namespace Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;

#if NETSTANDARD2_0 || NETSTANDARD2_1

internal static class HttpResponseMessageExtensions
{
    private const string STATUS_CODE_KEY_NAME = "StatusCode";

    extension<TSource>(IEnumerable<TSource> source)
    {
        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            count = 0;
            return false;
        }

        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            comparer ??= Comparer<TKey>.Default;

            using IEnumerator<TSource> e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                if (default(TSource) is null)
                {
                    return default;
                }
                else
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
            }

            TSource value = e.Current;
            TKey key = keySelector(value);

            if (default(TKey) is null)
            {
                if (key is null)
                {
                    TSource firstValue = value;

                    do
                    {
                        if (!e.MoveNext())
                        {
                            // All keys are null, surface the first element.
                            return firstValue;
                        }

                        value = e.Current;
                        key = keySelector(value);
                    }
                    while (key is null);
                }

                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (nextKey is not null && comparer.Compare(nextKey, key) > 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
            else
            {
                if (comparer == Comparer<TKey>.Default)
                {
                    while (e.MoveNext())
                    {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (Comparer<TKey>.Default.Compare(nextKey, key) > 0)
                        {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                }
                else
                {
                    while (e.MoveNext())
                    {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (comparer.Compare(nextKey, key) > 0)
                        {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                }
            }

            return value;
        }

        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            comparer ??= Comparer<TKey>.Default;

            using IEnumerator<TSource> e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                if (default(TSource) is null)
                {
                    return default;
                }
                else
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
            }

            TSource value = e.Current;
            TKey key = keySelector(value);

            if (default(TKey) is null)
            {
                if (key is null)
                {
                    TSource firstValue = value;

                    do
                    {
                        if (!e.MoveNext())
                        {
                            // All keys are null, surface the first element.
                            return firstValue;
                        }

                        value = e.Current;
                        key = keySelector(value);
                    }
                    while (key is null);
                }

                while (e.MoveNext())
                {
                    TSource nextValue = e.Current;
                    TKey nextKey = keySelector(nextValue);
                    if (nextKey is not null && comparer.Compare(nextKey, key) < 0)
                    {
                        key = nextKey;
                        value = nextValue;
                    }
                }
            }
            else
            {
                if (comparer == Comparer<TKey>.Default)
                {
                    while (e.MoveNext())
                    {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (Comparer<TKey>.Default.Compare(nextKey, key) < 0)
                        {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                }
                else
                {
                    while (e.MoveNext())
                    {
                        TSource nextValue = e.Current;
                        TKey nextKey = keySelector(nextValue);
                        if (comparer.Compare(nextKey, key) < 0)
                        {
                            key = nextKey;
                            value = nextValue;
                        }
                    }
                }
            }

            return value;
        }
    }

    extension(HttpRequestException httpRequestException)
    {
        public bool SetStatusCode(HttpStatusCode httpStatusCode)
        {
            httpRequestException.Data[STATUS_CODE_KEY_NAME] = httpStatusCode;

            return false;
        }

        public HttpStatusCode? GetStatusCode() => (HttpStatusCode?)httpRequestException.Data[STATUS_CODE_KEY_NAME];
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
        public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken) =>
            await httpContent.ReadAsStreamAsync().WaitAsync(
                timeout: Timeout.InfiniteTimeSpan,
                TimeProvider.System,
                cancellationToken: cancellationToken);

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken) =>
            await httpContent.ReadAsStringAsync().WaitAsync(
                timeout: Timeout.InfiniteTimeSpan,
                TimeProvider.System,
                cancellationToken: cancellationToken);
    }
}

#endif

#if NETSTANDARD2_0

internal static class Netstandard20Extensions
{
    extension<T>(IEnumerable<T> collection)
    {
        public HashSet<T> ToHashSet() => [.. collection];
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
        public bool Contains(string value, StringComparison comparisonType) => target.IndexOf(value, comparisonType) >= 0;
    }
}

#endif
