using Expressive.Expressions;

namespace Expressive.Operators.Bitwise;

internal class BitwiseAndOperator : OperatorBase
{
	public override string[] Tags => new string[1] { "&" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new BinaryExpression(BinaryExpressionType.BitwiseAnd, expressions[0], expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.BitwiseAnd;
	}
}
