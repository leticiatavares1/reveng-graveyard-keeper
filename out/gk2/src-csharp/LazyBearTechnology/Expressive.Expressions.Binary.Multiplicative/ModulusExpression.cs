using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Binary.Multiplicative;

internal class ModulusExpression : BinaryExpressionBase
{
	public ModulusExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, Numbers.Modulus);
	}
}
