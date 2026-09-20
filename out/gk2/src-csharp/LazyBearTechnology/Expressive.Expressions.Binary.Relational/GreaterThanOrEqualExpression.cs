using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Binary.Relational;

internal class GreaterThanOrEqualExpression : BinaryExpressionBase
{
	public GreaterThanOrEqualExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return Comparison.CompareUsingMostPreciseType(lhsResult, rightHandSide.Evaluate(variables), base.Context) >= 0;
	}
}
