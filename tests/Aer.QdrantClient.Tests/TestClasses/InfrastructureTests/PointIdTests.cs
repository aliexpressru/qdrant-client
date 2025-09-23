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
        new object[] {PointId.Integer(1), PointId.Guid(_firstGuid), false},
        new object[] {PointId.Integer(1), null, false},

        new object[] {PointId.Guid(_firstGuid), PointId.Guid(_firstGuid), true},
        new object[] {PointId.Guid(_firstGuid), PointId.Guid(_secondGuid), false},
        new object[] {PointId.Guid(_firstGuid), PointId.Integer(1), false},
        new object[] {PointId.Guid(_firstGuid), null, false},

        new object[] {null, null, true}
    ];

    public static object[] PointIdRawSources =
    [
        new object[] {"not a guid", null, true},
        new object[] {1.3, null, true},
        new object[] {-1, null, true},

        new object[] {null, PointId.NewGuid(), false},
        
        new object[] {"08ced0de-5a51-4162-b839-8fd8ab3c6b6c", PointId.Guid("08ced0de-5a51-4162-b839-8fd8ab3c6b6c"), false},
        new object[] {1, PointId.Integer(1), false},
        
        new object[] {10U, PointId.Integer(10U), false},
        new object[] {100L, PointId.Integer(100L), false},
    ];

    [Test]
    [TestCaseSource(nameof(PointIdCases))]
    public void Equality(PointId x, PointId y, bool shouldBeEqual)
    {
        if (x is not null)
        {
            x.Equals(y).Should().Be(shouldBeEqual);
        }

        (x == y).Should().Be(shouldBeEqual);
        (x != y).Should().Be(!shouldBeEqual);
    }

    [Test]
    public void ImplicitConversions()
    { 
        PointId pointIdFromInt = 42;
        PointId pointIdFromLong = 42L;
        PointId pointIdFromUlong = 42UL;
        PointId pointIdFromUint = 42U;

        IntegerPointId intPointIdFromInt = 42;
        IntegerPointId intPointIdFromLong = 42L;
        IntegerPointId intPointIdFromUlong = 42UL;
        IntegerPointId intPointIdFromUint = 42U;
        
        var expectedGuid = Guid.NewGuid();
        PointId pointIdFromGuid = expectedGuid;
        
        pointIdFromInt.Should().BeOfType<IntegerPointId>();
        ((IntegerPointId)pointIdFromInt).Id.Should().Be(42UL);

        pointIdFromLong.Should().BeOfType<IntegerPointId>();
        ((IntegerPointId) pointIdFromLong).Id.Should().Be(42UL);

        pointIdFromUlong.Should().BeOfType<IntegerPointId>();
        ((IntegerPointId) pointIdFromUlong).Id.Should().Be(42UL);

        pointIdFromUint.Should().BeOfType<IntegerPointId>();
        ((IntegerPointId) pointIdFromUint).Id.Should().Be(42UL);
        
        intPointIdFromInt.Id.Should().Be(42UL);
        intPointIdFromLong.Id.Should().Be(42UL);
        intPointIdFromUlong.Id.Should().Be(42UL);
        intPointIdFromUint.Id.Should().Be(42UL);
        
        pointIdFromGuid.Should().BeOfType<GuidPointId>();
        ((GuidPointId)pointIdFromGuid).Id.Should().Be(expectedGuid);
    }

    [Test]
    [TestCaseSource(nameof(PointIdRawSources))]
    public void Creation(object pointIdRawSource, PointId expected, bool shouldThrowOnCreation)
    {
        var pointIdCreateAct = () => PointId.Create(pointIdRawSource);

        if (shouldThrowOnCreation)
        {
            pointIdCreateAct.Should().Throw<QdrantInvalidPointIdException>();
        }
        else
        {
            // NewGuid case is treated specially, as it always returns a random guid

            pointIdCreateAct.Should().NotThrow();
            var createdPointId = pointIdCreateAct();
            
            if (pointIdRawSource is null
                && expected is GuidPointId)
            { 
                createdPointId.Should().BeOfType<GuidPointId>();
            }
            else
            {
                createdPointId.Equals(expected).Should().BeTrue();
            }
        }
    }

    [Test]
    public void StringRepresentation()
    {
        PointId intPointId = 42;
        IntegerPointId intPointId2 = 42;
        
        var expectedIntPointIdString = "Int: 42";
        
        PointId guidPointId = _firstGuid;
        GuidPointId guidPointId2 = _firstGuid;
        
        var expectedGuidPointIdString = $"Guid: {_firstGuid}";
        
        intPointId.ToString(includeTypeInfo: true).Should().Be(expectedIntPointIdString);
        intPointId2.ToString(includeTypeInfo: true).Should().Be(expectedIntPointIdString);
        
        guidPointId.ToString(includeTypeInfo: true).Should().Be(expectedGuidPointIdString);
        guidPointId2.ToString(includeTypeInfo: true).Should().Be(expectedGuidPointIdString);
    }
}
