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

        VectorBase denseVector = CreateTestVector((uint)vectorLength);
        VectorBase sparseVector = CreateTestSparseVector((uint)vectorLength, 5);

        Dictionary<string, float[]> namedVectorsRaw = new(2);

        foreach (var vectorName in CreateVectorNames(2, addDefaultVector: true))
        {
            var vector = CreateTestVector((uint)vectorLength);

            namedVectorsRaw.Add(vectorName, vector);
        }

        VectorBase namedVectors = namedVectorsRaw;

        VectorBase multivector = CreateTestMultivector((uint)vectorLength, 2, VectorDataType.Float32);

        VectorBase nullVector = null;

        denseVector.VectorKind.Should().Be(VectorKind.Dense);
        float[] denseVectorRaw = (float[])denseVector;
        denseVectorRaw.Length.Should().Be(vectorLength);

        sparseVector.VectorKind.Should().Be(VectorKind.Sparse);
        var convertSparseToFloatAct = () => (float[])sparseVector;
        convertSparseToFloatAct.Should().Throw<NotSupportedException>();

        namedVectors.VectorKind.Should().Be(VectorKind.Named);
        namedVectors.AsNamedVectors().Vectors.Count.Should().Be(2);
        namedVectors.Default.VectorKind.Should().Be(VectorKind.Dense);
        float[] namedDenseVectorRaw = (float[])namedVectors.Default;
        namedDenseVectorRaw.Length.Should().Be(vectorLength);

        multivector.VectorKind.Should().Be(VectorKind.Multi);
        multivector.Default.VectorKind.Should().Be(VectorKind.Dense);
        float[] multivectorRaw = (float[])multivector.Default;
        multivectorRaw.Length.Should().Be(vectorLength);

        // ReSharper disable once ExpressionIsAlwaysNull
        float[] nullVectorRaw = (float[])nullVector;
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
            Indices: new[] { 1U, 5U, 7U },
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

        VectorBase sparseVectorSingleComponent = (Indices: new[] { 5U }, Values: [0.345f]);

        sparseVectorSingleComponent.VectorKind.Should().Be(VectorKind.Sparse);

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

    [Test]
    public void GetVectorStringRepresentationToStream()
    {
        VectorBase denseVector = new[]
        {
            0.3667106f,
            0.6042991f,
            0.08220377f
        };

        AssertVectorStreamsContainsString(denseVector, "[0.3667106,0.6042991,0.08220377]");

        VectorBase sparseVector = (
            Indices: new[] { 1U, 5U, 7U },
            Values: [0.4f, 0.345f, 0.99f]
        );

        AssertVectorStreamsContainsString(
            sparseVector,
            """
            {
              "indexes" : [1, 5, 7],
              "values" : [0.4, 0.345, 0.99]
            }
            """);

        VectorBase sparseVectorSingleComponent = (Indices: new[] { 5U }, Values: [0.345f]);

        AssertVectorStreamsContainsString(
            sparseVectorSingleComponent,
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

        AssertVectorStreamsContainsString(
            multivectorSingleVector,
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
        AssertVectorStreamsContainsString(
            multivector,
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

        AssertVectorStreamsContainsString(
            namedVectors,
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

        AssertVectorStreamsContainsString(
            namedVectorsNestedNamed,
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

    [Test]
    public void VectorEqualityMembers()
    {
        DenseVector denseVector = new DenseVector()
        {
            VectorValues = [1, 2, 3]
        };

        DenseVector denseVectorEqual = new DenseVector()
        {
            VectorValues = [1, 2, 3]
        };

        DenseVector denseVectorNotEqual = new DenseVector()
        {
            VectorValues = [3, 4, 5]
        };

        SparseVector sparseVector = new SparseVector(new[] { 1U, 2U, 3U }, [1, 2, 3]);
        SparseVector sparseVectorEqual = new SparseVector(new[] { 1U, 2U, 3U }, [1, 2, 3]);
        SparseVector sparseVectorNotEqual = new SparseVector(new[] { 1U, 2U, 3U }, [3, 4, 5]);

        MultiVector multivector = new MultiVector()
        {
            Vectors = [[1, 2, 3], [4, 5, 6]]
        };

        MultiVector multivectorEqual = new MultiVector()
        {
            Vectors = [[1, 2, 3], [4, 5, 6]]
        };

        MultiVector multivectorNotEqual = new MultiVector()
        {
            Vectors = [[1, 2, 3], [7, 8, 9]]
        };

        NamedVectors namedVectors = new NamedVectors()
        {
            Vectors = new Dictionary<string, VectorBase>()
            {
                ["vec1"] = denseVector,
                ["vec2"] = sparseVector,
                ["vec3"] = multivector
            }
        };

        NamedVectors namedVectorsEqual = new NamedVectors()
        {
            Vectors = new Dictionary<string, VectorBase>()
            {
                ["vec1"] = denseVectorEqual,
                ["vec2"] = sparseVectorEqual,
                ["vec3"] = multivectorEqual
            }
        };

        NamedVectors namedVectorsNotEqual = new NamedVectors()
        {
            Vectors = new Dictionary<string, VectorBase>()
            {
                ["vec1"] = denseVectorNotEqual,
                ["vec2"] = sparseVectorNotEqual,
                ["vec3"] = multivectorNotEqual
            }
        };

        denseVector.Equals(denseVector).Should().BeTrue();
        denseVector.Equals(denseVectorEqual).Should().BeTrue();
        denseVector.Equals(denseVectorNotEqual).Should().BeFalse();
        denseVector.Equals((VectorBase)denseVectorEqual).Should().BeTrue();
        denseVector.Equals((VectorBase)denseVectorNotEqual).Should().BeFalse();
        denseVector.Equals((object)denseVectorEqual).Should().BeTrue();
        denseVector.Equals((object)denseVectorNotEqual).Should().BeFalse();

        denseVector.GetHashCode().Should().Be(denseVectorEqual.GetHashCode());

        sparseVector.Equals(sparseVector).Should().BeTrue();
        sparseVector.Equals(sparseVectorEqual).Should().BeTrue();
        sparseVector.Equals(sparseVectorNotEqual).Should().BeFalse();
        sparseVector.Equals((VectorBase)sparseVectorEqual).Should().BeTrue();
        sparseVector.Equals((VectorBase)sparseVectorNotEqual).Should().BeFalse();
        sparseVector.Equals((object)sparseVectorEqual).Should().BeTrue();
        sparseVector.Equals((object)sparseVectorNotEqual).Should().BeFalse();

        sparseVector.GetHashCode().Should().Be(sparseVectorEqual.GetHashCode());

        multivector.Equals(multivector).Should().BeTrue();
        multivector.Equals(multivectorEqual).Should().BeTrue();
        multivector.Equals(multivectorNotEqual).Should().BeFalse();
        multivector.Equals((VectorBase)multivectorEqual).Should().BeTrue();
        multivector.Equals((VectorBase)multivectorNotEqual).Should().BeFalse();
        multivector.Equals((object)multivectorEqual).Should().BeTrue();
        multivector.Equals((object)multivectorNotEqual).Should().BeFalse();

        multivector.GetHashCode().Should().Be(multivectorEqual.GetHashCode());

        namedVectors.Equals(namedVectors).Should().BeTrue();
        namedVectors.Equals(namedVectorsEqual).Should().BeTrue();
        namedVectors.Equals(namedVectorsNotEqual).Should().BeFalse();
        namedVectors.Equals((VectorBase)namedVectorsEqual).Should().BeTrue();
        namedVectors.Equals((VectorBase)namedVectorsNotEqual).Should().BeFalse();
        namedVectors.Equals((object)namedVectorsEqual).Should().BeTrue();
        namedVectors.Equals((object)namedVectorsNotEqual).Should().BeFalse();

        namedVectors.GetHashCode().Should().Be(namedVectorsEqual.GetHashCode());
    }

    private void AssertVectorStreamsContainsString(VectorBase vector, string expectedString)
    {
        // String stream representation
        using MemoryStream ms = new();
        using StreamWriter sw = new(ms);

        vector.WriteToStream(sw);
        sw.Flush();
        ms.Position = 0;

        using StreamReader sr = new(ms);
        var vectorString = sr.ReadToEnd();

        vectorString.AssertSameString(expectedString);

        // Since we don't have methods to read 

        using MemoryStream msBinary = new();
        using BinaryWriter bw = new(msBinary);

        vector.WriteToStream(bw);
        bw.Flush();
        msBinary.Position = 0;

        using BinaryReader br = new(msBinary);

        VectorBase vb = VectorBase.ReadFromStream(vector.VectorKind, br);

        var vectorStringFromBinary = vb.ToString();
        vectorStringFromBinary.AssertSameString(expectedString);
    }
}
