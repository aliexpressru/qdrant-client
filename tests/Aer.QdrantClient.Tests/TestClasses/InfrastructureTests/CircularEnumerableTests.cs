using Aer.QdrantClient.Http.Collections;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class CircularEnumerableTests
{
    [Test]
    public void TestCircularEnumerable_CircularEnumeration()
    {
        var ce = new CircularEnumerable<int>([1, 2, 3]);
        var first = ce.GetNext();
        var second = ce.GetNext();
        var third = ce.GetNext();
        var firstAgain = ce.GetNext();

        first.Should().Be(1);
        second.Should().Be(2);
        third.Should().Be(3);
        firstAgain.Should().Be(1);
    }

    [Test]
    public void TestCircularEnumerable_CircleDetection_OneElement()
    {
        var ce = new CircularEnumerable<int>([1]);

        ce.GetNext(); // 1 <- start detecting circle here

        using var _ = ce.StartCircleDetection();

        var actCircle = () => ce.GetNext(); // 1 again - circle detected

        actCircle.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TestCircularEnumerable_CircleDetection_FromStart()
    {
        var ce = new CircularEnumerable<int>([1,2,3]);

        using var _ = ce.StartCircleDetection();

        ce.GetNext(); // 1 <- start detecting circle here
        ce.GetNext(); // 2
        var act = () => ce.GetNext(); // 3
        var actCircle = () => ce.GetNext(); // 1 again - circle detected

        act.Should().NotThrow();
        actCircle.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TestCircularEnumerable_CircleDetection_FromEnd()
    {
        var ce = new CircularEnumerable<int>([1, 2, 3]);

        ce.GetNext(); // 1
        ce.GetNext(); // 2
        ce.GetNext(); // 3

        using var _ = ce.StartCircleDetection();

        ce.GetNext(); // 1
        ce.GetNext(); // 2
        var actCircle = () => ce.GetNext(); // 3 again - circle detected

        actCircle.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TestCircularEnumerable_CircleDetection_ThenSimpleEnumeration()
    {
        var ce = new CircularEnumerable<int>([1, 2, 3]);

        ce.GetNext(); // 1
        ce.GetNext(); // 2 <- start detecting circle here

        var circle = ce.StartCircleDetection();

        ce.GetNext(); // 3
        ce.GetNext(); // 1
        var actCircle = () => ce.GetNext(); // 2 again - circle detected
        var actCircle2 = () => ce.GetNext(); // 2 again - circle detected

        actCircle.Should().Throw<InvalidOperationException>();
        actCircle2.Should().Throw<InvalidOperationException>();

        // dispose circle detector
        circle.Dispose();

        // resume normal enumeration
        var nextElement = ce.GetNext();

        nextElement.Should().Be(2);
    }

    [Test]
    public void TestCircularEnumerable_CircleDetection_DoubleDetection()
    {
        var ce = new CircularEnumerable<int>([1, 2, 3]);

        ce.GetNext(); // 1
        ce.GetNext(); // 2 <- start detecting circle here

        var firstCircleDetector = ce.StartCircleDetection();

        ce.GetNext(); // 3 <- reset circle detection to here

        ce.StartCircleDetection();

        // dispose of the first detector - should not affect behavior
        firstCircleDetector.Dispose();

        ce.GetNext(); // 1
        var secondAct = () => ce.GetNext(); // 2 - should not throw since we started a new circle
        secondAct.Should().NotThrow();

        var thirdAct = () => ce.GetNext(); // 3 again - should throw
        thirdAct.Should().Throw<InvalidOperationException>();
    }
}
