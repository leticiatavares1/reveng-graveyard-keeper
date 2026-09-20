using System;
using System.Collections.Generic;
using Expressive.Helpers;

namespace Expressive.Expressions.Binary.Multiplicative;

internal class DivideExpression : BinaryExpressionBase
{
	public DivideExpression(IExpression lhs, IExpression rhs, Context context)
		: base(lhs, rhs, context)
	{
	}

	protected override object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables)
	{
		return BinaryExpressionBase.EvaluateAggregates(lhsResult, rightHandSide, variables, (object l, object r) => (l != null && r != null && !IsReal(l) && !IsReal(r)) ? Numbers.Divide(Convert.ToDouble(l), r) : Numbers.Divide(l, r));
	}

	private static bool IsReal(object value)
	{
		TypeCode typeCode = TypeHelper.GetTypeCode(value);
		if (typeCode != TypeCode.Decimal && typeCode != TypeCode.Double)
		{
			return typeCode == TypeCode.Single;
		}
		return true;
	}
}
