using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class Log10Function : FunctionBase
{
	public override string Name => "Log10";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Log10(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
