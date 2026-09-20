using System.Collections.Generic;

namespace Expressive.Expressions;

internal class ConstantValueExpression : IExpression
{
	private readonly object value;

	internal ConstantValueExpression(object value)
	{
		this.value = value;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		return value;
	}
}
