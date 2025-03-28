namespace Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;

#if NETSTANDARD2_0
internal static class Netstandard20Extensions
{
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection) => [..collection];

	public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key))
		{
			return false;
		}

		dictionary.Add(key, value);
		return true;
	}

	public static void Deconstruct<TKey, TValue>(
		this KeyValuePair<TKey, TValue> target,
		out TKey key,
		out TValue value)
	{
		key = target.Key;
		value = target.Value;
	}

	public static bool Contains(this string target, string value, StringComparison comparisonType)
	{
		return target.IndexOf(value, comparisonType) >= 0;
	}
}
#endif