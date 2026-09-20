using Expressive.Expressions;

namespace Expressive.Operators.Bitwise;

internal class BitwiseOrOperator : OperatorBase
{
	public override string[] Tags => new string[1] { "|" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new BinaryExpression(BinaryExpressionType.BitwiseOr, expressions[0], expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.BitwiseOr;
	}
}
