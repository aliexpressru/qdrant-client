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
}
