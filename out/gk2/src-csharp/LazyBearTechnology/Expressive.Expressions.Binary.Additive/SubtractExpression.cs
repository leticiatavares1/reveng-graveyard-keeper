using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Binary.Additive;

internal class SubtractExpression : BinaryExpressionBase
{
	public SubtractExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, Numbers.Subtract);
	}
}
