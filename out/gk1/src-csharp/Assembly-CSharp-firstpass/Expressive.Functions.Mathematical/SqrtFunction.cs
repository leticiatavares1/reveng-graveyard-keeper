using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class SqrtFunction : FunctionBase
{
	public override string Name => "Sqrt";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Sqrt(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
