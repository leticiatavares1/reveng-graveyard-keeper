using System;
using Expressive.Expressions;

namespace Expressive.Functions.Conversion;

internal sealed class DecimalFunction : FunctionBase
{
	public override string Name => "Decimal";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		return Convert.ToDecimal(obj, context.CurrentCulture);
	}
}
