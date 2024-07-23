using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class PointIdTests
{
    private static readonly Guid _firstGuid = Guid.NewGuid();
    private static readonly Guid _secondGuid = Guid.NewGuid();

    public static object[] PointIdCases =
    [
        new object[] {PointId.Integer(1), PointId.Integer(1), true},
        new object[] {PointId.Integer(1), PointId.Integer(2), false},
        new object[] {PointId.Integer(1), null, false},

        new object[] {PointId.Guid(_firstGuid), PointId.Guid(_firstGuid), true},
        new object[] {PointId.Guid(_firstGuid), PointId.Guid(_secondGuid), false},
        new object[] {PointId.Guid(_firstGuid), null, false},

        new object[] {null, null, true}
    ];

    [Test]
    [TestCaseSource(nameof(PointIdCases))]
    public void TestPointIdEquality(PointId x, PointId y, bool shouldBeEqual)
    {
        if (x is not null)
        {
            x.Equals(y).Should().Be(shouldBeEqual);
        }

        (x == y).Should().Be(shouldBeEqual);
        (x != y).Should().Be(!shouldBeEqual);
    }
}
