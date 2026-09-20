using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class TanFunction : FunctionBase
{
	public override string Name => "Tan";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Tan(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
