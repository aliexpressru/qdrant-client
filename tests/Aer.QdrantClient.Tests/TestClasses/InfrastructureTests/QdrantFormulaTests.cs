using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Formulas;
using Aer.QdrantClient.Http.Formulas.Builders;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests;

internal class QdrantFormulaTests
{
	[Test]
	public void Constant()
	{
		QdrantFormula doubleConstant = F.Constant(10.1);

		var formulaString = doubleConstant.ToString();

		var expectedFormula = """
			10.1
			""";

		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());

		QdrantFormula stringConstant = F.Constant("test");

		expectedFormula = """
			"test"
			""";

		formulaString = stringConstant.ToString();

		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}
	
	[Test]
	public void FilterCondition()
	{
		QdrantFormula formula = F.Filter(Q.MatchValue("field", 1567));

		var formulaString = formula.ToString();

		var expectedFormula = """
			{
			  "key": "field",
			  "match": {
			    "value": 1567
			  }
			}
			""";

		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}

	[Test]
	public void PrefetchReference()
	{
		QdrantFormula defaultReference = F.PrefetchReference();
		QdrantFormula specificReference = F.PrefetchReference(15);
		
		var defaultFormulaString = defaultReference.ToString();
		
		var expectedDefaultFormula = """
			"$score"
			""";

		var specificFormulaString = specificReference.ToString();
		
		var expectedSpecificFormula = """
			"$score[15]"
			""";
		
		defaultFormulaString.Should().Be(expectedDefaultFormula.ReplaceLineEndings());
		specificFormulaString.Should().Be(expectedSpecificFormula.ReplaceLineEndings());
	}

	[Test]
	public void CollectionExpression()
	{
		QdrantFormula formula = F.Sum(
			F.Constant(1.0),
			F.PrefetchReference(),
			F.Multiply(
				F.Constant("abc"),
				F.Abs(
					F.Filter(
						Q.MatchAny("test_payload", 1, 23)
					)
				)
			)
		);
		
		var formulaString = formula.ToString();
		
		var expectedFormula = """
			{
			  "sum": [
			    1,
			    "$score",
			    {
			      "mult": [
			        "abc",
			        {
			          "abs": {
			            "key": "test_payload",
			            "match": {
			              "any": [
			                1,
			                23
			              ]
			            }
			          }
			        }
			      ]
			    }
			  ]
			}
			""";
		
		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}

	
	[Test]
	public void Divide()
	{
		QdrantFormula formula =
			F.Divide(
				F.Abs(-12),
				F.Sum(
					F.PrefetchReference(),
					F.Constant(3.0)
				),
				-1
			);
		
		var formulaString = formula.ToString();
		
		var expectedFormula = """
			{
			  "div": {
			    "left": {
			      "abs": -12
			    },
			    "right": {
			      "sum": [
			        "$score",
			        3
			      ]
			    },
			    "by_zero_default": -1
			  }
			}
			""";
		
		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}

	[Test]
	public void Power()
	{
		QdrantFormula formula =
			F.Power(
				F.PrefetchReference(3),
				F.Constant(2.0)
			);
		
		var formulaString = formula.ToString();
		
		var expectedFormula = """
			{
			  "pow": {
			    "base": "$score[3]",
			    "exponent": 2
			  }
			}
			""";
		
		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}

	[Test]
	public void UnaryExpression()
	{
		QdrantFormula formula = F.Sqrt(
			F.Negate(
				F.Exponent(
					F.Log10(
						F.Ln(
							F.Filter(
								Q.BeInRange(
									"test",
									greaterThanOrEqual: 1,
									lessThanOrEqual: 23)
							)
						)
					)
				)
			)
		);
		
		var formulaString = formula.ToString();
		
		var expectedFormula = """
			{
			  "sqrt": {
			    "neg": {
			      "exp": {
			        "log10": {
			          "ln": {
			            "key": "test",
			            "range": {
			              "lte": 23,
			              "gte": 1
			            }
			          }
			        }
			      }
			    }
			  }
			}
			""";
		
		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}

	[Test]
	public void ValueExpression()
	{
		QdrantFormula dtKeyFormula = F.DateTimeKey("test");
		QdrantFormula dtValueFormula = F.DateTimeValue("2023-10-01T00:00:00Z");
		
		var dtKeyFormulaString = dtKeyFormula.ToString();
		var expectedDtKeyFormula = """
			{
			  "datetime_key": "test"
			}
			""";
		
		dtKeyFormulaString.Should().Be(expectedDtKeyFormula.ReplaceLineEndings());
		var dtValueFormulaString = dtValueFormula.ToString();
		var expectedDtValueFormula = """
			{
			  "datetime": "2023-10-01T00:00:00Z"
			}
			""";
		dtValueFormulaString.Should().Be(expectedDtValueFormula.ReplaceLineEndings());
	}

	[Test]
	public void GeoDistanceExpression()
	{
		QdrantFormula formula = F.GeoDistance(
				24.56,
				12.34,
				"geo_field");
		
		var formulaString = formula.ToString();
		var expectedFormula = """
			{
			  "geo_distance": {
			    "origin": {
			      "lon": 24.56,
			      "lat": 12.34
			    },
			    "to": "geo_field"
			  }
			}
			""";
		
		formulaString.Should().Be(expectedFormula.ReplaceLineEndings());
	}

	[Test]
	public void DecayExpression()
	{
		QdrantFormula gaussDecayFormula = F.GaussDecay(
			"test_field",
			10.0,
			5.0,
			2.0);

		QdrantFormula linearDecayFormula = F.LinearDecay(
			F.Sum(
				F.PrefetchReference(),
				10),
			F.Filter(
				Q.MatchAny(
					"test_field",
					1,
					2,
					3
				)
			)
		);
		
		QdrantFormula exponentialDecayFormula = F.ExponentialDecay(
			F.Constant(5.0),
			"test_field",
			midpoint: 10
		);
		
		var gaussDecayFormulaString = gaussDecayFormula.ToString();
		var expectedGaussDecayFormula = """
			{
			  "gauss_decay": {
			    "x": "test_field",
			    "target": 10,
			    "scale": 5,
			    "midpoint": 2
			  }
			}
			""";
		gaussDecayFormulaString.Should().Be(expectedGaussDecayFormula.ReplaceLineEndings());
		
		var linearDecayFormulaString = linearDecayFormula.ToString();
		var expectedLinearDecayFormula = """
			{
			  "lin_decay": {
			    "x": {
			      "sum": [
			        "$score",
			        10
			      ]
			    },
			    "target": {
			      "key": "test_field",
			      "match": {
			        "any": [
			          1,
			          2,
			          3
			        ]
			      }
			    }
			  }
			}
			""";
		linearDecayFormulaString.Should().Be(expectedLinearDecayFormula.ReplaceLineEndings());
		
		var exponentialDecayFormulaString = exponentialDecayFormula.ToString();
		var expectedExponentialDecayFormula = """
			{
			  "exp_decay": {
			    "x": 5,
			    "target": "test_field",
			    "midpoint": 10
			  }
			}
			""";
		
		exponentialDecayFormulaString.Should().Be(expectedExponentialDecayFormula.ReplaceLineEndings());
	}
}
