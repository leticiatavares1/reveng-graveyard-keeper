using System;
using System.Collections.Generic;

namespace Expressive.Expressions.Binary.Logical;

internal class OrExpression : BinaryExpressionBase
{
	public OrExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return Convert.ToBoolean(lhsResult) || Convert.ToBoolean(rightHandSide.Evaluate(variables));
	}
}
