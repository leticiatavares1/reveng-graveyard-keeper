using System.Collections.Generic;
using Expressive.Expressions;
using Expressive.Expressions.Binary.Logical;

namespace Expressive.Operators.Logical;

internal class AndOperator : OperatorBase
{
	public override IEnumerable<string> Tags => new string[2] { "&&", "and" };

	public override IExpression BuildExpression(Token previousToken, IExpression[] expressions, Context context)
	{
		return new AndExpression(expressions[0], expressions[1], context);
	}

	public override OperatorPrecedence GetPrecedence(Token previousToken)
	{
		return OperatorPrecedence.And;
	}
}
