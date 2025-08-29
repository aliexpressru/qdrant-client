using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Helpers;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class VectorTests : QdrantTestsBase
{
    [Test]
    public void ConvertVectorTypes()
    {
        int vectorLength = 10;

        VectorBase denseVector = CreateTestVector((uint) vectorLength);
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

    [Test]
    public void GetVectorStringRepresentation()
    {
        VectorBase denseVector = new[]
        {
            0.3667106f,
            0.6042991f,
            0.08220377f
        };

        denseVector.VectorKind.Should().Be(VectorKind.Dense);

        var denseVectorString = denseVector.ToString();

        denseVectorString.AssertSameString("[0.3667106,0.6042991,0.08220377]");

        VectorBase sparseVector = (
            Indices: new[] {1U, 5U, 7U},
            Values: [0.4f, 0.345f, 0.99f]
        );

        sparseVector.VectorKind.Should().Be(VectorKind.Sparse);

        var sparseVectorString = sparseVector.ToString();

        sparseVectorString.AssertSameString(
            """
            {
              "indexes" : [1, 5, 7],
              "values" : [0.4, 0.345, 0.99]
            }
            """
        );

        VectorBase sparseVectorSingleComponent = new SparseVector(
            new[] {5U},
            [0.345f]
        );

        var sparseVectorSingleComponentString = sparseVectorSingleComponent.ToString();

        sparseVectorSingleComponentString.AssertSameString(
            """
            {
              "indexes" : [5],
              "values" : [0.345]
            }
            """);

        float[][] multivectorSingleVectorRaw =
        [
            [
                0.3667106f,
                0.6042991f,
                0.08220377f
            ]
        ];

        VectorBase multivectorSingleVector = multivectorSingleVectorRaw;

        multivectorSingleVector.VectorKind.Should().Be(VectorKind.Multi);

        var multivectorSingleVectorString = multivectorSingleVector.ToString();

        multivectorSingleVectorString.AssertSameString(
            """
            [ 
                [0.3667106,0.6042991,0.08220377] 
            ]
            """);

        float[][] multivectorRaw =
        [
            [
                0.3667106f,
                0.6042991f,
                0.08220377f
            ],
            [
                0.1f,
                0.2f,
                0.3f
            ],
            [
                0.9f,
                0.8f,
                0.7f
            ]
        ];

        VectorBase multivector = multivectorRaw;

        multivector.VectorKind.Should().Be(VectorKind.Multi);

        var multivectorString = multivector.ToString();

        multivectorString.AssertSameString(
            """
            [
                [
                    0.3667106,
                    0.6042991,
                    0.08220377
                ],
                [
                    0.1,
                    0.2,
                    0.3
                ],
                [
                    0.9,
                    0.8,
                    0.7
                ]
            ]
            """);

        VectorBase namedVectors = new Dictionary<string, VectorBase>()
        {
            ["Dense"] = denseVector,
            ["Sparse"] = sparseVector,
            ["SparseSingle"] = sparseVectorSingleComponent,
            ["MultivectorSingle"] = multivectorSingleVector,
            ["Multivector"] = multivector
        };

        namedVectors.VectorKind.Should().Be(VectorKind.Named);

        var namedVectorsString = namedVectors.ToString();

        namedVectorsString.AssertSameString(
            """
            {
              "Dense" : [ 0.3667106, 0.6042991, 0.08220377 ],
              "Sparse" : {
                "indexes" : [ 1, 5, 7 ],
                "values" : [ 0.4, 0.345, 0.99 ]
              },
              "SparseSingle" : {
                "indexes" : [ 5 ],
                "values" : [ 0.345 ]
              },
              "MultivectorSingle" : [ [ 0.3667106, 0.6042991, 0.08220377 ] ],
              "Multivector" : [ [ 0.3667106, 0.6042991, 0.08220377 ], [ 0.1, 0.2, 0.3 ], [ 0.9, 0.8, 0.7 ] ]
            }
            """);

        VectorBase namedVectorsNestedNamed = new Dictionary<string, VectorBase>()
        {
            ["Named"] = namedVectors
        };
        
        namedVectorsNestedNamed.VectorKind.Should().Be(VectorKind.Named);
        
        var namedVectorsNestedNamedString = namedVectorsNestedNamed.ToString();
        
        namedVectorsNestedNamedString.AssertSameString(
            """
            {
              "Named" : {
                "Dense" : [ 0.3667106, 0.6042991, 0.08220377 ],
                "Sparse" : {
                  "indexes" : [ 1, 5, 7 ],
                  "values" : [ 0.4, 0.345, 0.99 ]
                },
                "SparseSingle" : {
                  "indexes" : [ 5 ],
                  "values" : [ 0.345 ]
                },
                "MultivectorSingle" : [ [ 0.3667106, 0.6042991, 0.08220377 ] ],
                "Multivector" : [ [ 0.3667106, 0.6042991, 0.08220377 ], [ 0.1, 0.2, 0.3 ], [ 0.9, 0.8, 0.7 ] ]
              }
            }
            """);
    }
}
