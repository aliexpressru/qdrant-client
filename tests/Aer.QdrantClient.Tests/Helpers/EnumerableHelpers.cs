namespace Aer.QdrantClient.Tests.Helpers;

internal static class EnumerableHelpers
{
    public static IEnumerable<T> YieldSingle<T>(this T source)
    {
        yield return source;
    }
}
