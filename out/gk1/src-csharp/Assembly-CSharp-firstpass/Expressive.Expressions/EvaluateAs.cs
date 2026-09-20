using System;
using System.Collections.Generic;

namespace Expressive.Expressions;

public static class EvaluateAs
{
	public static string EvaluateAsString(this IExpression e, IDictionary<string, object> variables)
	{
		return Convert.ToString(e.Evaluate(variables));
	}

	public static int EvaluateAsInt(this IExpression e, IDictionary<string, object> variables)
	{
		return Convert.ToInt32(e.Evaluate(variables));
	}

	public static bool EvaluateAsBoolean(this IExpression e, IDictionary<string, object> variables)
	{
		return Convert.ToBoolean(e.Evaluate(variables));
	}

	public static float EvaluateAsFloat(this IExpression e, IDictionary<string, object> variables)
	{
		return Convert.ToSingle(e.Evaluate(variables));
	}
}
