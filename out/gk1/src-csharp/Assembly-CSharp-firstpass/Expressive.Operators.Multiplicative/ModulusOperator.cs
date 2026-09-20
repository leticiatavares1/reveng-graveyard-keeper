using Expressive.Expressions;

namespace Expressive.Operators.Multiplicative;

internal class ModulusOperator : OperatorBase
{
	public override string[] Tags => new string[2] { "%", "mod" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new BinaryExpression(BinaryExpressionType.Modulus, expressions[0], expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.Modulus;
	}
}
