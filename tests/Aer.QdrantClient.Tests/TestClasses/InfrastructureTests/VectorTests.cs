using Aer.QdrantClient.Http.Models.Primitives.Inference;
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

        VectorBase documentVector = InferenceObject.CreateFromDocument(
            "Test text",
            "test-model",
            options: new()
            {
                ["api-key"] = "test"
            },
            bm25Options: new()
            {
                B = 10,
                K = 10,
                Tokenizer = FullTextIndexTokenizerType.Prefix,
                Stemmer = FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.English),
                Language = "English",
                AsciiFolding = true,
                AvgLen = 10,
                MaxTokenLen = 10,
                MinTokenLen = 10,
                Lowercase = true
            }
        );

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

        documentVector.VectorKind.Should().Be(VectorKind.Inferred);
        documentVector.Default.VectorKind.Should().Be(VectorKind.Inferred);
        documentVector.AsInferredVector().InferenceObject.Should().NotBeNull();
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

        VectorBase imageVector = InferenceObject.CreateFromImage(
            "test",
            "test-model",
            new()
            {
                ["api-key"] = "test",
                ["some-other-value"] = "test2",
            }
        );

        var imageVectorString = imageVector.ToString();

        imageVectorString.AssertSameString(
            """
            {
                "InferenceObject": {
                    "image": "test",
                    "model": "test-model",
                    "options": {
                        "api-key": "test",
                        "some-other-value": "test2"
                    }
                }
            }
            """);

        VectorBase documentVector = InferenceObject.CreateFromDocument(
            "Test text",
            "test-model",
            options: new()
            {
                ["api-key"] = "test"
            },
            bm25Options: new()
            {
                B = 10,
                K = 10,
                Tokenizer = FullTextIndexTokenizerType.Prefix,
                Stemmer = FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.English),
                Language = "English",
                AsciiFolding = true,
                AvgLen = 10,
                MaxTokenLen = 10,
                MinTokenLen = 10,
                Lowercase = true
            }
        );

        var documentVectorString = documentVector.ToString();

        documentVectorString.AssertSameString(
            """
            {
                "InferenceObject": {
                    "text": "Test text",
                    "bm25_options": {
                        "k": 10,
                        "b": 10,
                        "avg_len": 10,
                        "tokenizer": "prefix",
                        "language": "English",
                        "lowercase": true,
                        "ascii_folding": true,
                        "stemmer": {
                            "type": "snowball",
                            "language": "english"
                        },
                        "min_token_len": 10,
                        "max_token_len": 10
                    },
                    "model": "test-model",
                    "options": {
                        "api-key": "test"
                    }
                }
            }
            """);

        VectorBase objectVector = InferenceObject.CreateFromObject(
            new TestObject(1),
            "test-model",
            new()
            {
                ["api-key"] = "test"
            }
        );

        var objectVectorString = objectVector.ToString();

        objectVectorString.AssertSameString(
            """
            {
                "InferenceObject": {
                    "object": {
                        "a": 1
                    },
                    "model": "test-model",
                    "options": {
                        "api-key": "test"
                    }
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

        AssertVectorStreamContainsString(denseVector, "[0.3667106,0.6042991,0.08220377]");

        VectorBase sparseVector = (
            Indices: new[] { 1U, 5U, 7U },
            Values: [0.4f, 0.345f, 0.99f]
        );

        AssertVectorStreamContainsString(
            sparseVector,
            """
            {
              "indexes" : [1, 5, 7],
              "values" : [0.4, 0.345, 0.99]
            }
            """);

        VectorBase sparseVectorSingleComponent = (Indices: new[] { 5U }, Values: [0.345f]);

        AssertVectorStreamContainsString(
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

        AssertVectorStreamContainsString(
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
        AssertVectorStreamContainsString(
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

        AssertVectorStreamContainsString(
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

        AssertVectorStreamContainsString(
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

        VectorBase imageVector = InferenceObject.CreateFromImage(
            "test",
            "test-model",
            new()
            {
                ["api-key"] = "test",
                ["some-other-value"] = "test2",
            }
        );

        AssertVectorStreamContainsString(
            imageVector,
            """
            {
                "InferenceObject": {
                    "image": "test",
                    "model": "test-model",
                    "options": {
                        "api-key": "test",
                        "some-other-value": "test2"
                    }
                }
            }
            """);

        VectorBase documentVector = InferenceObject.CreateFromDocument(
            "Test text",
            "test-model",
            options: new()
            {
                ["api-key"] = "test"
            },
            bm25Options: new()
            {
                B = 10,
                K = 10,
                Tokenizer = FullTextIndexTokenizerType.Prefix,
                Stemmer = FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.English),
                Language = "English",
                AsciiFolding = true,
                AvgLen = 10,
                MaxTokenLen = 10,
                MinTokenLen = 10,
                Lowercase = true
            }
        );

        AssertVectorStreamContainsString(
            documentVector,
            """
            {
                "InferenceObject": {
                    "text": "Test text",
                    "bm25_options": {
                        "k": 10,
                        "b": 10,
                        "avg_len": 10,
                        "tokenizer": "prefix",
                        "language": "English",
                        "lowercase": true,
                        "ascii_folding": true,
                        "stemmer": {
                            "type": "snowball",
                            "language": "english"
                        },
                        "min_token_len": 10,
                        "max_token_len": 10
                    },
                    "model": "test-model",
                    "options": {
                        "api-key": "test"
                    }
                }
            }
            """);

        VectorBase objectVector = InferenceObject.CreateFromObject(
            new TestObject(1),
            "test-model",
            new()
            {
                ["api-key"] = "test"
            }
        );

        AssertVectorStreamContainsString(
            objectVector,
            """
            {
                "InferenceObject": {
                    "object": {
                        "a": 1
                    },
                    "model": "test-model",
                    "options": {
                        "api-key": "test"
                    }
                }
            }
            """);
    }

    [Test]
    public void VectorEqualityMembers()
    {
        DenseVector denseVector = new([1, 2, 3]);

        DenseVector denseVectorEqual = new([1, 2, 3]);

        DenseVector denseVectorNotEqual = new([3, 4, 5]);

#pragma warning disable CA1861 // Avoid constant arrays as arguments | Justification : Test code
        SparseVector sparseVector = new(new[] { 1U, 2U, 3U }, [1, 2, 3]);
        SparseVector sparseVectorEqual = new(new[] { 1U, 2U, 3U }, [1, 2, 3]);
        SparseVector sparseVectorNotEqual = new(new[] { 1U, 2U, 3U }, [3, 4, 5]);
#pragma warning restore CA1861 // Avoid constant arrays as arguments

        MultiVector multivector = new()
        {
            Vectors = [[1, 2, 3], [4, 5, 6]]
        };

        MultiVector multivectorEqual = new()
        {
            Vectors = [[1, 2, 3], [4, 5, 6]]
        };

        MultiVector multivectorNotEqual = new()
        {
            Vectors = [[1, 2, 3], [7, 8, 9]]
        };

        NamedVectors namedVectors = new()
        {
            Vectors = new Dictionary<string, VectorBase>()
            {
                ["vec1"] = denseVector,
                ["vec2"] = sparseVector,
                ["vec3"] = multivector
            }
        };

        NamedVectors namedVectorsEqual = new()
        {
            Vectors = new Dictionary<string, VectorBase>()
            {
                ["vec1"] = denseVectorEqual,
                ["vec2"] = sparseVectorEqual,
                ["vec3"] = multivectorEqual
            }
        };

        NamedVectors namedVectorsNotEqual = new()
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

    [Test]
    public void InferredVectorEqualityMembers()
    {
        InferredVector imageVector = InferenceObject.CreateFromImage(
            "test",
            "test-model",
            new()
            {
                ["api-key"] = "test"
            }
        );

        InferredVector imageVectorEqual = InferenceObject.CreateFromImage(
            "test",
            "test-model",
            new()
            {
                ["api-key"] = "test"
            }
        );

        InferredVector imageVectorNotEqual = InferenceObject.CreateFromImage(
            "test",
            "test-model",
            new()
            {
                ["api-key"] = "test",
                ["some-other-key"] = "some-value" // Difference
            }
        );

        InferredVector documentVector = InferenceObject.CreateFromDocument(
            "Test text",
            "test-model",
            options: new()
            {
                ["api-key"] = "test"
            },
            bm25Options: new()
            {
                B = 10,
                K = 10,
                Tokenizer = FullTextIndexTokenizerType.Prefix,
                Stemmer = FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.English),
                Language = "English",
                AsciiFolding = true,
                AvgLen = 10,
                MaxTokenLen = 10,
                MinTokenLen = 10,
                Lowercase = true
            }
        );

        InferredVector documentVectorEqual = InferenceObject.CreateFromDocument(
            "Test text",
            "test-model",
            options: new()
            {
                ["api-key"] = "test",
                ["some-other-key"] = "some-value" // Difference but since both vectors have bm25Options we should ignore comparing Options components
            },
            bm25Options: new()
            {
                B = 10,
                K = 10,
                Tokenizer = FullTextIndexTokenizerType.Prefix,
                Stemmer = FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.English),
                Language = "English",
                AsciiFolding = true,
                AvgLen = 10,
                MaxTokenLen = 10,
                MinTokenLen = 10,
                Lowercase = true
            }
        );

        InferredVector documentVectorNotEqual = InferenceObject.CreateFromDocument(
            "Test text",
            "test-model",
            options: new()
            {
                ["api-key"] = "test" // Again, not comparing Options since Bm25 exist
            },
            bm25Options: new()
            {
                B = 10,
                K = 10,
                Tokenizer = FullTextIndexTokenizerType.Prefix,
                Stemmer = FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.Italian), // Difference
                Language = "English",
                AsciiFolding = true,
                AvgLen = 10,
                MaxTokenLen = 10,
                MinTokenLen = 10,
                Lowercase = true
            }
        );

        InferredVector objectVector = InferenceObject.CreateFromObject(
            new TestObject(1), // using records since they implement equality members
            "test-model",
            new()
            {
                ["api-key"] = "test"
            }
        );

        InferredVector objectVectorEqual = InferenceObject.CreateFromObject(
            new TestObject(1),
            "test-model",
            new()
            {
                ["api-key"] = "test"
            }
        );

        InferredVector objectVectorNotEqual = InferenceObject.CreateFromObject(
            new TestObject(1),
            "test-model",
            new()
            {
                ["api-key"] = "test1" // Difference
            }
        );

        imageVector.Equals(imageVector).Should().BeTrue();
        imageVector.Equals(imageVectorEqual).Should().BeTrue();
        imageVector.Equals(imageVectorNotEqual).Should().BeFalse();
        imageVector.Equals((object)imageVectorEqual).Should().BeTrue();
        imageVector.Equals((object)imageVectorNotEqual).Should().BeFalse();

        imageVector.GetHashCode().Should().Be(imageVectorEqual.GetHashCode());

        documentVector.Equals(documentVector).Should().BeTrue();
        documentVector.Equals(documentVectorEqual).Should().BeTrue();
        documentVector.Equals(documentVectorNotEqual).Should().BeFalse();
        documentVector.Equals((object)documentVectorEqual).Should().BeTrue();
        documentVector.Equals((object)documentVectorNotEqual).Should().BeFalse();

        documentVector.GetHashCode().Should().Be(documentVectorEqual.GetHashCode());

        objectVector.Equals(objectVector).Should().BeTrue();
        objectVector.Equals(objectVectorEqual).Should().BeTrue();
        objectVector.Equals(objectVectorNotEqual).Should().BeFalse();
        objectVector.Equals((object)objectVectorEqual).Should().BeTrue();
        objectVector.Equals((object)objectVectorNotEqual).Should().BeFalse();

        objectVector.GetHashCode().Should().Be(objectVectorEqual.GetHashCode());
    }

    private record TestObject(int A);

    private static void AssertVectorStreamContainsString(VectorBase vector, string expectedString)
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
    }
}
