using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class QdrantFilterTests
{
    [Test]
    public void TestQdrantFilter_ImplicitlyCreateMustTopLevelCondition()
    {
        QdrantFilter filter = Q.MatchValue("whatever", 1);

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "whatever",
                  "match": {
                    "value": 1
                  }
                }
              ]
            }
            """;

        filterString.Should().Be(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_EmptyFilter()
    {
        var filter = QdrantFilter.Empty.ToString();

        filter.Should().Be("");
    }

    [Test]
    public void TestQdrantFilter_Must_Match()
    {
        var filter = QdrantFilter.Create(
            Q.Must(
                Q<TestPayload>.MatchValue(p => p.Integer, 123),
                Q<TestPayload>.MatchValue(p => p.Text, "test_value")
            )
        ).ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "integer",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "text",
                  "match": {
                    "value": "test_value"
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Should_Match()
    {
        var filter = QdrantFilter.Create(
                Q.Should(
                    Q.MatchValue("test_key", 123),
                    Q.MatchValue("test_key_2", "test_value")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "should": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "test_key_2",
                  "match": {
                    "value": "test_value"
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_MinShould_Match()
    {
        var filter = QdrantFilter.Create(
                Q.MinShould(
                    1,
                    Q.MatchValue("test_key", 123),
                    Q.MatchValue("test_key_2", "test_value")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "min_should": {
                "min_count": 1,
                "conditions": [
                  {
                    "key": "test_key",
                    "match": {
                      "value": 123
                    }
                  },
                  {
                    "key": "test_key_2",
                    "match": {
                      "value": "test_value"
                    }
                  }
                ]
              }
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_MustNot_Match_SingleField()
    {
        var filter = QdrantFilter.Create(
                Q.MustNot(
                    Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must_not": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 123
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_MustNot_Match()
    {
        var filter = QdrantFilter.Create(
                Q.MustNot(
                    Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123),
                    Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must_not": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "text",
                  "match": {
                    "value": "test_value"
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_MatchExcept()
    {
        var filter = QdrantFilter.Create(
            Q.Must(
                Q<TestPayload>.MatchExceptValue(p => p.Integer, 123, 234),
                Q<TestPayload>.MatchExceptValue(p => p.Text, "test_value", "test_value_2")
            )
        ).ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "integer",
                  "match": {
                    "except": [
                      123,
                      234
                    ]
                  }
                },
                {
                  "key": "text",
                  "match": {
                    "except": [
                      "test_value",
                      "test_value_2"
                    ]
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_And_MustNot_Match_SingleField()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
                ),
                Q.MustNot(
                    Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 123
                  }
                }
              ],
              "must_not": [
                {
                  "key": "text",
                  "match": {
                    "value": "test_value"
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_PlusEqualsOperatorCombined()
    {
        QdrantFilter filter = null;

        filter += Q.Must(
            Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
        );

        FilterConditionBase combinedCondition = null;

        combinedCondition += Q.MustNot(
            Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
        );

        combinedCondition += Q.Should(
            Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
        );

        filter += combinedCondition;

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 123
                  }
                }
              ],
              "must_not": [
                {
                  "key": "text",
                  "match": {
                    "value": "test_value"
                  }
                }
              ],
              "should": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 234
                  }
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_PlusOperatorCombined()
    {
        QdrantFilter filter =
            Q.Must(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
            )
            +
            Q.MustNot(
                Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
            )
            +
            Q.Should(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 123
                  }
                }
              ],
              "must_not": [
                {
                  "key": "text",
                  "match": {
                    "value": "test_value"
                  }
                }
              ],
              "should": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 234
                  }
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_AndOperatorCombined()
    {
        QdrantFilter filter =
            Q.Must(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
            )
            &
            Q.MustNot(
                Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
            )
            &
            Q.Should(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "must_not": [
                    {
                      "key": "text",
                      "match": {
                        "value": "test_value"
                      }
                    }
                  ]
                },
                {
                  "should": [
                    {
                      "key": "int_property",
                      "match": {
                        "value": 234
                      }
                    }
                  ]
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_OrOperatorCombined()
    {
        QdrantFilter filter =
            Q.Must(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
            )
            |
            Q.MustNot(
                Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
            )
            |
            Q.Should(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "should": [
                {
                  "must": [
                    {
                      "key": "int_property",
                      "match": {
                        "value": 123
                      }
                    }
                  ]
                },
                {
                  "must_not": [
                    {
                      "key": "text",
                      "match": {
                        "value": "test_value"
                      }
                    }
                  ]
                },
                {
                  "key": "int_property",
                  "match": {
                    "value": 234
                  }
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_AndOrOperatorCombined()
    {
        // note that operator associativity is modified by parethesis
        QdrantFilter filter =
            (
                Q.Must(
                    Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
                )
                |
                Q.MustNot(
                    Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
                )
            )
            &
            Q.Should(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "should": [
                    {
                      "must": [
                        {
                          "key": "int_property",
                          "match": {
                            "value": 123
                          }
                        }
                      ]
                    },
                    {
                      "must_not": [
                        {
                          "key": "text",
                          "match": {
                            "value": "test_value"
                          }
                        }
                      ]
                    }
                  ]
                },
                {
                  "should": [
                    {
                      "key": "int_property",
                      "match": {
                        "value": 234
                      }
                    }
                  ]
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_PlusAndOrOperatorCombined()
    {
        // note that operator associativity is modified by parethesis
        QdrantFilter filter =
            (
                (
                    Q.Must(
                        Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
                    )
                    &
                    Q.MustNot(
                        Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
                    )
                )
                |
                Q.Should(
                    Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
                )
            )
            +
            Q.Must(
                Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 567)
            )
            +
            Q.MustNot(
                (
                    Q.Must(
                        Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 123)
                    )
                    &
                    Q.MustNot(
                        Q<TestComplexPayload>.MatchValue(p => p.Text, "test_value")
                    )
                )
                |
                Q.Should(
                    Q<TestComplexPayload>.MatchValue(p => p.IntProperty, 234)
                )
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "should": [
                {
                  "must": [
                    {
                      "key": "int_property",
                      "match": {
                        "value": 123
                      }
                    },
                    {
                      "must_not": [
                        {
                          "key": "text",
                          "match": {
                            "value": "test_value"
                          }
                        }
                      ]
                    }
                  ]
                },
                {
                  "key": "int_property",
                  "match": {
                    "value": 234
                  }
                }
              ],
              "must": [
                {
                  "key": "int_property",
                  "match": {
                    "value": 567
                  }
                }
              ],
              "must_not": [
                {
                  "should": [
                    {
                      "must": [
                        {
                          "key": "int_property",
                          "match": {
                            "value": 123
                          }
                        },
                        {
                          "must_not": [
                            {
                              "key": "text",
                              "match": {
                                "value": "test_value"
                              }
                            }
                          ]
                        }
                      ]
                    },
                    {
                      "key": "int_property",
                      "match": {
                        "value": 234
                      }
                    }
                  ]
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_Negated()
    {
        QdrantFilter filter =
                !Q.Must(
                    Q.MatchValue("test_key", 123),
                    Q.MatchValue("test_key_1", Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b176"))
                );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must_not": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "test_key_1",
                  "match": {
                    "value": "e0d28537-03ed-43a8-b93c-b1c54371b176"
                  }
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_MustNot_Negated()
    {
        QdrantFilter filter =
            !Q.MustNot(
                Q.MatchValue("test_key", 123),
                Q.MatchValue("test_key_1", Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b176"))
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "test_key_1",
                  "match": {
                    "value": "e0d28537-03ed-43a8-b93c-b1c54371b176"
                  }
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_Negated_Unwrapped()
    {
        QdrantFilter filter =
            !(
                Q.MatchValue("test_key", 123)
                &
                Q.MatchValue("test_key_1", Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b176"))
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must_not": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "test_key_1",
                  "match": {
                    "value": "e0d28537-03ed-43a8-b93c-b1c54371b176"
                  }
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Condition_Negated()
    {
        QdrantFilter filter =
            Q.Must(
                Q.MatchValue("test_key", 123),
                !Q.MatchValue("test_key_1", Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b176"))
            );

        var filterString = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "must_not": [
                    {
                      "key": "test_key_1",
                      "match": {
                        "value": "e0d28537-03ed-43a8-b93c-b1c54371b176"
                      }
                    }
                  ]
                }
              ]
            }
            """;

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_And_MustNot_Match()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q.MatchValue("test_key", 123),
                    Q.MatchValue("test_key_1", Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b176"))
                ),
                Q.MustNot(
                    Q.MatchValue("test_key_2", "test_value"),
                    Q.MatchValue("test_key_3", 15.67)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "test_key_1",
                  "match": {
                    "value": "e0d28537-03ed-43a8-b93c-b1c54371b176"
                  }
                }
              ],
              "must_not": [
                {
                  "key": "test_key_2",
                  "match": {
                    "value": "test_value"
                  }
                },
                {
                  "key": "test_key_3",
                  "match": {
                    "value": 15.67
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_Nested_MustNot_Match()
    {
        var filter = QdrantFilter.Create(
                Q.MustNot(
                    Q.Must(
                        Q.MatchValue("test_key", 123),
                        Q.MatchValue("test_key_2", "test_value")
                    )
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must_not": [
                {
                  "must": [
                    {
                      "key": "test_key",
                      "match": {
                        "value": 123
                      }
                    },
                    {
                      "key": "test_key_2",
                      "match": {
                        "value": "test_value"
                      }
                    }
                  ]
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_ComplexNesting_Unwarpping()
    {
        var filter = QdrantFilter.Create(
            Q.Must(
                Q.Must(
                    Q.MatchValue("test_key", 123),
                    Q.MatchValue("test_key_2", "test_value")
                )
                +
                !Q.MatchValue("test_key_3", 15.67)
            ),
            !(
                Q.Must(
                    Q.MatchValue("test_key", 123),
                    Q.MatchValue("test_key_2", "test_value")
                )
                |
                !Q.MatchValue("test_key_3", 15.67)
            )
            ,
            Q.Should(
                Q.MatchValue("test_key_4", -1),
                Q.Should(
                    Q.MatchValue("test_key_5", 1567),
                    Q.MatchValue("test_key_6", 1568)
                )
            )
        ).ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "test_key",
                  "match": {
                    "value": 123
                  }
                },
                {
                  "key": "test_key_2",
                  "match": {
                    "value": "test_value"
                  }
                },
                {
                  "must_not": [
                    {
                      "key": "test_key_3",
                      "match": {
                        "value": 15.67
                      }
                    }
                  ]
                }
              ],
              "must_not": [
                {
                  "should": [
                    {
                      "must": [
                        {
                          "key": "test_key",
                          "match": {
                            "value": 123
                          }
                        },
                        {
                          "key": "test_key_2",
                          "match": {
                            "value": "test_value"
                          }
                        }
                      ]
                    },
                    {
                      "must_not": [
                        {
                          "key": "test_key_3",
                          "match": {
                            "value": 15.67
                          }
                        }
                      ]
                    }
                  ]
                }
              ],
              "should": [
                {
                  "key": "test_key_4",
                  "match": {
                    "value": -1
                  }
                },
                {
                  "key": "test_key_5",
                  "match": {
                    "value": 1567
                  }
                },
                {
                  "key": "test_key_6",
                  "match": {
                    "value": 1568
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_MatchAny()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.MatchAny(p => p.Nested.Integer, 123, 345),
                    Q<TestComplexPayload>.MatchAny(p => p.Nested.Name, "test_value", "test_value_2")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "nested.integer",
                  "match": {
                    "any": [
                      123,
                      345
                    ]
                  }
                },
                {
                  "key": "nested.name",
                  "match": {
                    "any": [
                      "test_value",
                      "test_value_2"
                    ]
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_Match_FultText()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.MatchSubstring(p => p.Text, "test_substring"),
                    Q<TestComplexPayload>.MatchValue(p => p.Nested.Name, "test_value")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "text",
                  "match": {
                    "text": "test_substring"
                  }
                },
                {
                  "key": "nested.name",
                  "match": {
                    "value": "test_value"
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_Range()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.BeInRange(p => p.IntProperty, greaterThanOrEqual: 1, lessThanOrEqual: 100),
                    Q<TestComplexPayload>.BeInRange(p => p.Nested.Double, greaterThanOrEqual: 1.1, lessThanOrEqual: 100.2),
                    // these are impossible conditions but they are used to test all the parameters
                    Q<TestComplexPayload>.BeInRange(
                        p => p.FloatingPointNumber,
                        greaterThan: 1.1,
                        greaterThanOrEqual: 12.2,
                        lessThan: 100.3,
                        lessThanOrEqual: 1000.4),
                    Q<TestComplexPayload>.BeInRange(
                        p => p.Array,
                        greaterThan: 1,
                        greaterThanOrEqual: 12,
                        lessThan: 100,
                        lessThanOrEqual: 1000)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "int_property",
                  "range": {
                    "lte": 100,
                    "gte": 1
                  }
                },
                {
                  "key": "nested.double",
                  "range": {
                    "lte": 100.2,
                    "gte": 1.1
                  }
                },
                {
                  "key": "floating_point_number",
                  "range": {
                    "lt": 100.3,
                    "lte": 1000.4,
                    "gt": 1.1,
                    "gte": 12.2
                  }
                },
                {
                  "key": "array",
                  "range": {
                    "lt": 100,
                    "lte": 1000,
                    "gt": 1,
                    "gte": 12
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_ValuesCount()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.HaveValuesCount(
                        p => p.Array,
                        greaterThanOrEqual: 1,
                        lessThanOrEqual: 100),
                    // this is an impossible condition but is used to test all the parameters
                    Q<TestComplexPayload>.HaveValuesCount(
                        p => p.Array,
                        greaterThan: 1,
                        greaterThanOrEqual: 12,
                        lessThan: 100,
                        lessThanOrEqual: 1000)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "array",
                  "values_count": {
                    "lte": 100,
                    "gte": 1
                  }
                },
                {
                  "key": "array",
                  "values_count": {
                    "lt": 100,
                    "lte": 1000,
                    "gt": 1,
                    "gte": 12
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_IsEmpty()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.BeNullOrEmpty(p => p.Text),
                    Q<TestComplexPayload>.HaveValuesCount(p => p.Array, greaterThanOrEqual: 1, lessThanOrEqual: 100)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "is_empty": {
                    "key": "text"
                  }
                },
                {
                  "key": "array",
                  "values_count": {
                    "lte": 100,
                    "gte": 1
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_IsNull()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.BeNull(p => p.IntProperty),
                    Q<TestComplexPayload>.BeNullOrEmpty(p => p.Nested.Name)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "is_null": {
                    "key": "int_property"
                  }
                },
                {
                  "is_empty": {
                    "key": "nested.name"
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_HaveId()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q.HaveAnyId(
                        Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b176"),
                        Guid.Parse("e0d28537-03ed-43a8-b93c-b1c54371b178")),
                    Q.HaveAnyId(PointId.Integer(123), PointId.Integer(234)),
                    Q.HaveAnyId(456, 678),
                    Q.HaveAnyId("e0d28537-03ed-43a8-b93c-b1c54371b180", "e0d28537-03ed-43a8-b93c-b1c54371b181")
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "has_id": [
                    "e0d28537-03ed-43a8-b93c-b1c54371b176",
                    "e0d28537-03ed-43a8-b93c-b1c54371b178"
                  ]
                },
                {
                  "has_id": [
                    123,
                    234
                  ]
                },
                {
                  "has_id": [
                    456,
                    678
                  ]
                },
                {
                  "has_id": [
                    "e0d28537-03ed-43a8-b93c-b1c54371b180",
                    "e0d28537-03ed-43a8-b93c-b1c54371b181"
                  ]
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_GeoBoundingBox()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.BeWithinGeoBoundingBox(
                        p => p.Location,
                        13.403683,
                        52.520711,
                        13.455868,
                        52.495862)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "location",
                  "geo_bounding_box": {
                    "bottom_right": {
                      "lat": 52.495862,
                      "lon": 13.455868
                    },
                    "top_left": {
                      "lat": 52.520711,
                      "lon": 13.403683
                    }
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_GeoRadius()
    {
        var filter = QdrantFilter.Create(
                Q.Must(
                    Q<TestComplexPayload>.BeWithinGeoRadius(
                        p => p.Location,
                        13.403683,
                        52.520711,
                        1000.1)
                )
            )
            .ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "location",
                  "geo_radius": {
                    "center": {
                      "lat": 52.520711,
                      "lon": 13.403683
                    },
                    "radius": 1000.1
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_GeoPolygon()
    {
        var filter = QdrantFilter.Create(
            Q.Must(
                Q<TestComplexPayload>.BeWithinGeoPolygon(
                    p => p.Location,
                    new[]
                    {
                        new GeoPoint(-70.1, -70.1),
                        new GeoPoint(-70.1, 60.1),
                        new GeoPoint(60.1, 60.1),
                        new GeoPoint(60.1, -70.1),
                        new GeoPoint(-70.1, -70.1)
                    },
                    new[]
                    {
                        new GeoPoint(-65.1, -65.1),
                        new GeoPoint(-65.1, 0.1),
                        new GeoPoint(0.1, 0.1),
                        new GeoPoint(0.1, -65.1),
                        new GeoPoint(-65.1, -65.1),
                    },
                    new[]
                    {
                        new GeoPoint(-75.1, -75.1),
                        new GeoPoint(-75.1, 10.1),
                        new GeoPoint(10.1, 10.1),
                        new GeoPoint(10.1, -75.1),
                        new GeoPoint(-75.1, -75.1),
                    })
            )
        );

        var filterJson = filter.ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "key": "location",
                  "geo_polygon": {
                    "exterior": {
                      "points": [
                        {
                          "lat": -70.1,
                          "lon": -70.1
                        },
                        {
                          "lat": -70.1,
                          "lon": 60.1
                        },
                        {
                          "lat": 60.1,
                          "lon": 60.1
                        },
                        {
                          "lat": 60.1,
                          "lon": -70.1
                        },
                        {
                          "lat": -70.1,
                          "lon": -70.1
                        }
                      ]
                    },
                    "interiors": [
                      {
                        "points": [
                          {
                            "lat": -65.1,
                            "lon": -65.1
                          },
                          {
                            "lat": -65.1,
                            "lon": 0.1
                          },
                          {
                            "lat": 0.1,
                            "lon": 0.1
                          },
                          {
                            "lat": 0.1,
                            "lon": -65.1
                          },
                          {
                            "lat": -65.1,
                            "lon": -65.1
                          }
                        ]
                      },
                      {
                        "points": [
                          {
                            "lat": -75.1,
                            "lon": -75.1
                          },
                          {
                            "lat": -75.1,
                            "lon": 10.1
                          },
                          {
                            "lat": 10.1,
                            "lon": 10.1
                          },
                          {
                            "lat": 10.1,
                            "lon": -75.1
                          },
                          {
                            "lat": -75.1,
                            "lon": -75.1
                          }
                        ]
                      }
                    ]
                  }
                }
              ]
            }
            """;

        filterJson.Should().NotBeNull();
        filterJson.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Must_NestedConditions_Match()
    {
        var filter = QdrantFilter.Create(
            Q.Must(
                Q<TestComplexPayload>.SatisfyNested(
                    p => p.Nested,
                    Q.Must(
                        Q<TestComplexPayload>.BeInRange(n => n.Nested.Integer, greaterThanOrEqual: 2),
                        Q<TestComplexPayload>.BeInRange(n => n.Nested.Double, greaterThan: 500, lessThanOrEqual: 1000)
                    )
                )
            )
        ).ToString();

        var expectedFilter = """
            {
              "must": [
                {
                  "nested": {
                    "key": "nested",
                    "filter": {
                      "must": [
                        {
                          "key": "nested.integer",
                          "range": {
                            "gte": 2
                          }
                        },
                        {
                          "key": "nested.double",
                          "range": {
                            "lte": 1000,
                            "gt": 500
                          }
                        }
                      ]
                    }
                  }
                }
              ]
            }
            """;

        filter.Should().NotBeNull();
        filter.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Should_Use_Raw_Filter_String_As_Is()
    {
      var expectedFilter = """
        {
          "must": [
            {
              "key": "integer",
              "match": {
                "value": 123
              }
            },
            {
              "key": "text",
              "match": {
                "value": "test_value"
              }
            }
          ]
        }
        """;

        var filter = QdrantFilter.Create(expectedFilter);
        var filterString = filter.ToString();

        filterString.Should().NotBeNull();
        filterString.Should().BeEquivalentTo(expectedFilter.ReplaceLineEndings());
    }

    [Test]
    public void TestQdrantFilter_Should_Throw_Exception_If_Condition_Is_Added_When_Use_Raw_Filter_String()
    {
      var expectedFilter = """
        {
          "must": [
            {
              "key": "integer",
              "match": {
                "value": 123
              }
            },
            {
              "key": "text",
              "match": {
                "value": "test_value"
              }
            }
          ]
        }
        """;

        var filter = QdrantFilter.Create(expectedFilter);

        var func = () => filter += new HasAnyIdCondition(Enumerable.Range(1, 5));

        func.Should().Throw<QdrantFilterModificationForbiddenException>();
    }
}
