namespace Aer.QdrantClient.Tests.Helpers;

internal static class StringHelper
{
    public static void AssertSameString(this string target, string expected)
    {
        if (expected is null)
        {
            target.Should().BeNull();
        }

        if (expected is not null)
        {
            target.Should().NotBeNull();
        }

        var compactActualValue = target.Compact();
        var compactExpectedValue = expected.Compact();

        compactActualValue.Should().BeEquivalentTo(compactExpectedValue);
    }

    public static void AssertContainsString(this string target, string expectedSubstring)
    {
        if (expectedSubstring is null)
        {
            target.Should().BeNull();
        }

        if (expectedSubstring is not null)
        {
            target.Should().NotBeNull();
        }

        var compactActualValue = target.Compact();
        var compactExpectedValue = expectedSubstring.Compact();

        compactActualValue.Should().Contain(compactExpectedValue);
    }

    private static string Compact(this string target)
        =>
            target
                .Replace("\t", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(" ", "");
}
