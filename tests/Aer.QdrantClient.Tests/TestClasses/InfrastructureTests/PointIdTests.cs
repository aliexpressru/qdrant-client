using Aer.QdrantClient.Http.Exceptions;
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

    public static object[] PointIdRawSources =
    [
        new object[] {"not a guid", null, true},
        new object[] {1.3, null, true},
        new object[] {-1, null, true},
        
        new object[] {"08ced0de-5a51-4162-b839-8fd8ab3c6b6c", PointId.Guid("08ced0de-5a51-4162-b839-8fd8ab3c6b6c"), false},
        new object[] {1, PointId.Integer(1), false},
        
        new object[] {10U, PointId.Integer(10U), false},
        new object[] {100L, PointId.Integer(100L), false},
    ];

    [Test]
    [TestCaseSource(nameof(PointIdCases))]
    public void PointIdEquality(PointId x, PointId y, bool shouldBeEqual)
    {
        if (x is not null)
        {
            x.Equals(y).Should().Be(shouldBeEqual);
        }

        (x == y).Should().Be(shouldBeEqual);
        (x != y).Should().Be(!shouldBeEqual);
    }

    [Test]
    [TestCaseSource(nameof(PointIdRawSources))]
    public void PointIdCreation(object pointIdRawSource, PointId expected, bool shouldThrowOnCreation)
    {
        var pointIdCreateAct = () => PointId.Create(pointIdRawSource);

        if (shouldThrowOnCreation)
        {
            pointIdCreateAct.Should().Throw<QdrantInvalidPointIdException>();
        }
        else
        {
            pointIdCreateAct.Should().NotThrow();
            
            var createdPointId = pointIdCreateAct();
            
            createdPointId.Equals(expected).Should().BeTrue();
        }
    }
}
