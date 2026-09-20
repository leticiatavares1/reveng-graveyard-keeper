using Expressive.Expressions;

namespace Expressive.Operators.Bitwise;

internal class RightShiftOperator : OperatorBase
{
	public override string[] Tags => new string[1] { ">>" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new BinaryExpression(BinaryExpressionType.RightShift, expressions[0], expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.LeftShift;
	}
}
