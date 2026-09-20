using System;
using Expressive.Expressions;

namespace Expressive.Functions.Conversion;

internal sealed class DateFunction : FunctionBase
{
	public override string Name => "Date";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		if (parameters.Length > 1 && obj is string s && parameters[1].Evaluate(base.Variables) is string format)
		{
			return DateTime.ParseExact(s, format, context.CurrentCulture);
		}
		return Convert.ToDateTime(obj, context.CurrentCulture);
	}
}
