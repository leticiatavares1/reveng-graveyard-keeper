using System;
using System.Collections.Generic;

namespace Expressive.Expressions;

internal class VariableExpression : IExpression
{
	private readonly string variableName;

	internal VariableExpression(string variableName)
	{
		this.variableName = variableName;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		if (variables == null || !variables.TryGetValue(variableName, out var value))
		{
			throw new ArgumentException("The variable '" + variableName + "' has not been supplied.");
		}
		if (value is IExpression expression)
		{
			return expression.Evaluate(variables);
		}
		return value;
	}
}
