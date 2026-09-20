using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class CosFunction : FunctionBase
{
	public override string Name => "Cos";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Cos(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
