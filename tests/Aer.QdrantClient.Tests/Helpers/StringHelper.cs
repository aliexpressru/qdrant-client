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

		var compactTarget = target.Compact();
		var compactExpected = expected.Compact();
		
		compactTarget.Should().BeEquivalentTo(compactExpected);
	}

	private static string Compact(this string target)
		=>
			target
				.Replace("\t", "")
				.Replace("\n", "")
				.Replace("\r", "")
				.Replace(" ", "");
}
