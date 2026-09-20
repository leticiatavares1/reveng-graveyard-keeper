using System.Collections.Generic;

namespace Expressive.Expressions.Binary.Conditional;

internal class NullCoalescingExpression : BinaryExpressionBase
{
	public NullCoalescingExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, (object l, object r) => l ?? r);
	}
}
