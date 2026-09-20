using System;
using Expressive.Expressions;

namespace Expressive.Functions.Date;

internal sealed class SecondOfFunction : FunctionBase
{
	public override string Name => "SecondOf";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		return Convert.ToDateTime(obj, context.CurrentCulture).Second;
	}
}
