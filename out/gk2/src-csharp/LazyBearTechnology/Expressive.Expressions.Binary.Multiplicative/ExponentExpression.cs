using System;
using System.Collections.Generic;

namespace Expressive.Expressions.Binary.Multiplicative;

internal class ExponentExpression : BinaryExpressionBase
{
	public ExponentExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, (object l, object r) => Math.Pow(Convert.ToDouble(l), Convert.ToDouble(r)));
	}
}
