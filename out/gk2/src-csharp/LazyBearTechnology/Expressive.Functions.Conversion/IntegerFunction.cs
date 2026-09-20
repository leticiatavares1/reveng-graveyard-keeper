using System;
using Expressive.Expressions;

namespace Expressive.Functions.Conversion;

internal sealed class IntegerFunction : FunctionBase
{
	public override string Name => "Integer";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		return Convert.ToInt32(obj, context.CurrentCulture);
	}
}
