using System;
using Expressive.Expressions;

namespace Expressive.Functions.Date;

internal sealed class AddYearsFunction : FunctionBase
{
	public override string Name => "AddYears";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 2, 2);
		object obj = parameters[0].Evaluate(base.Variables);
		object obj2 = parameters[1].Evaluate(base.Variables);
		if (obj == null || obj2 == null)
		{
			return null;
		}
		DateTime dateTime = Convert.ToDateTime(obj, context.CurrentCulture);
		int value = Convert.ToInt32(obj2, context.CurrentCulture);
		return dateTime.AddYears(value);
	}
}
