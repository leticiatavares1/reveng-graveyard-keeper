using System.Collections.Generic;

namespace Expressive.Expressions.Unary;

internal abstract class UnaryExpressionBase : IExpression
{
	protected readonly IExpression expression;

	internal UnaryExpressionBase(IExpression expression)
	{
		this.expression = expression;
	}

	public abstract object Evaluate(IDictionary<string, object> variables);
}
