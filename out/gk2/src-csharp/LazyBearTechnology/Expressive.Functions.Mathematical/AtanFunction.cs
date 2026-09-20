using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class AtanFunction : FunctionBase
{
	public override string Name => "Atan";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Atan(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
