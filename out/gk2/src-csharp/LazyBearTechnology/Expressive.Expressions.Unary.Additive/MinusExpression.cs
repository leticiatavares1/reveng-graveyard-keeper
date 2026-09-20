using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Unary.Additive;

internal class MinusExpression : UnaryExpressionBase
{
	public MinusExpression(IExpression expression)
		: base(expression)
	{
	}

	public override object Evaluate(IDictionary<string, object> variables)
	{
		return Numbers.Subtract(0, expression.Evaluate(variables));
	}
}
