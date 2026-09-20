using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class TruncateFunction : FunctionBase
{
	public override string Name => "Truncate";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Truncate(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
