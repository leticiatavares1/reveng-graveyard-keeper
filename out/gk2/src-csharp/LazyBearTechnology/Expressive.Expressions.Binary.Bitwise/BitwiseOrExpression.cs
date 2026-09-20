using System;
using System.Collections.Generic;

namespace Expressive.Expressions.Binary.Bitwise;

internal class BitwiseOrExpression : BinaryExpressionBase
{
	public BitwiseOrExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, (object l, object r) => Convert.ToUInt16(l) | Convert.ToUInt16(r));
	}
}
