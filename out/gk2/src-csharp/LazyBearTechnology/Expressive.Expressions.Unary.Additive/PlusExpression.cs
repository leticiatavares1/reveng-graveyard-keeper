using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Unary.Additive;

internal class PlusExpression : UnaryExpressionBase
{
	public PlusExpression(IExpression expression)
		: base(expression)
	{
	}

	public override object Evaluate(IDictionary<string, object> variables)
	{
		return Numbers.Add(0, expression.Evaluate(variables));
	}
}
