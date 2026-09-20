using Expressive.Expressions;

namespace Expressive.Operators.Logic;

internal class OrOperator : OperatorBase
{
	public override string[] Tags => new string[2] { "||", "or" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions)
	{
		return new BinaryExpression(BinaryExpressionType.Or, expressions[0], expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.Or;
	}
}
