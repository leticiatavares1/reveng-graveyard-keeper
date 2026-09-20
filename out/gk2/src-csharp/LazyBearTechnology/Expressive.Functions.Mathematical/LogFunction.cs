using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class LogFunction : FunctionBase
{
	public override string Name => "Log";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 2, 2);
		return Math.Log(Convert.ToDouble(parameters[0].Evaluate(base.Variables)), Convert.ToDouble(parameters[1].Evaluate(base.Variables)));
	}
}
