using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Binary.Additive;

internal class AddExpression : BinaryExpressionBase
{
	public AddExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		if (lhsResult is string text)
		{
			return text + rightHandSide.Evaluate(variables);
		}
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, Numbers.Add);
	}
}
