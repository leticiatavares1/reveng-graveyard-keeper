using System;
using Expressive.Expressions;

namespace Expressive.Functions.Logical;

internal class IfFunction : FunctionBase
{
	public override string Name => "If3";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 3, 3);
		if (!Convert.ToBoolean(parameters[0].Evaluate(base.Variables)))
		{
			return parameters[2].Evaluate(base.Variables);
		}
		return parameters[1].Evaluate(base.Variables);
	}
}
