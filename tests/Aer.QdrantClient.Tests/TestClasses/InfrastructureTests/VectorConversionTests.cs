using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class VectorConversionTests : QdrantTestsBase
{
    [Test]
    public void TestVectorConversions()
    {
        int vectorLength = 10;

        VectorBase denseVector = CreateTestVector((uint)vectorLength);
        VectorBase sparseVector = CreateTestSparseVector((uint) vectorLength, 5);

        Dictionary<string, float[]> namedVectorsRaw = new(2);

        foreach (var vectorName in CreateVectorNames(2, addDefaultVector: true))
        {
            var vector = CreateTestVector((uint) vectorLength);

            namedVectorsRaw.Add(vectorName, vector);
        }

        VectorBase namedVectors = namedVectorsRaw;

        VectorBase multivector = CreateTestMultivector((uint) vectorLength, 2, VectorDataType.Float32);

        VectorBase nullVector = null;

        denseVector.VectorKind.Should().Be(VectorKind.Dense);
        float[] denseVectorRaw = (float[]) denseVector;
        denseVectorRaw.Length.Should().Be(vectorLength);

        sparseVector.VectorKind.Should().Be(VectorKind.Sparse);
        var convertSparseToFloatAct = () => (float[]) sparseVector;
        convertSparseToFloatAct.Should().Throw<NotSupportedException>();

        namedVectors.VectorKind.Should().Be(VectorKind.Named);
        namedVectors.AsNamedVectors().Vectors.Count.Should().Be(2);
        namedVectors.Default.VectorKind.Should().Be(VectorKind.Dense);
        float[] namedDenseVectorRaw = (float[]) namedVectors.Default;
        namedDenseVectorRaw.Length.Should().Be(vectorLength);

        multivector.VectorKind.Should().Be(VectorKind.Multi);
        multivector.Default.VectorKind.Should().Be(VectorKind.Dense);
        float[] multivectorRaw = (float[]) multivector.Default;
        multivectorRaw.Length.Should().Be(vectorLength);

        // ReSharper disable once ExpressionIsAlwaysNull
        float[] nullVectorRaw = (float[]) nullVector;
        nullVectorRaw.Should().BeNull();
    }
}
