using System;
using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions;

internal class UnaryExpression : IExpression
{
	private readonly IExpression _expression;

	private readonly UnaryExpressionType _expressionType;

	internal UnaryExpression(UnaryExpressionType type, IExpression expression)
	{
		_expressionType = type;
		_expression = expression;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		switch (_expressionType)
		{
		case UnaryExpressionType.Minus:
			return Numbers.Subtract(0, _expression.Evaluate(variables));
		case UnaryExpressionType.Not:
		{
			object obj = _expression.Evaluate(variables);
			if (obj != null)
			{
				ReflectionTools.GetTypeCode(obj);
				if (obj is bool)
				{
					return !(bool)obj;
				}
				return !Convert.ToBoolean(obj);
			}
			break;
		}
		case UnaryExpressionType.Plus:
			return Numbers.Add(0, _expression.Evaluate(variables));
		}
		return null;
	}
}
