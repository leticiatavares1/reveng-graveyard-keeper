using Expressive.Expressions;

namespace Expressive.Operators.Multiplicative;

internal class MultiplyOperator : OperatorBase
{
	public override string[] Tags => new string[2] { "*", "×" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new BinaryExpression(BinaryExpressionType.Multiply, expressions[0], expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.Multiply;
	}
}
