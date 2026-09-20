using System.Collections.Generic;
using Expressive.Expressions;
using Expressive.Expressions.Unary.Logical;

namespace Expressive.Operators.Logical;

internal class NotOperator : OperatorBase
{
	public override IEnumerable<string> Tags => new string[2] { "!", "not" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions, Context context)
	{
		return new NotExpression(expressions[0] ?? expressions[1]);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.Not;
	}
}
