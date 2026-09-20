using System.Collections.Generic;
using Expressive.Expressions;
using Expressive.Expressions.Binary.Logical;

namespace Expressive.Operators.Logical;

internal class OrOperator : OperatorBase
{
	public override IEnumerable<string> Tags => new string[2] { "||", "or" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions, Context context)
	{
		return new OrExpression(expressions[0], expressions[1], context);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.Or;
	}
}
