using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Binary.Relational;

internal class NotEqualExpression : BinaryExpressionBase
{
	public NotEqualExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		if (lhsResult == null)
		{
			return rightHandSide.Evaluate(variables) != null;
		}
		object obj = rightHandSide.Evaluate(variables);
		if (obj == null)
		{
			return true;
		}
		return Comparison.CompareUsingMostPreciseType(lhsResult, obj, base.Context) != 0;
	}
}
