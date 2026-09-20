using System;
using System.Collections.Generic;

namespace Expressive.Expressions.Unary.Logical;

internal class NotExpression : UnaryExpressionBase
{
	public NotExpression(IExpression expression)
		: base(expression)
	{
	}

	public override object Evaluate(IDictionary<string, object> variables)
	{
		object obj = expression.Evaluate(variables);
		if (obj != null)
		{
			if (obj is bool flag)
			{
				return !flag;
			}
			return !Convert.ToBoolean(obj);
		}
		return null;
	}
}
