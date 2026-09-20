using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class FloorFunction : FunctionBase
{
	public override string Name => "Floor";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 1, 1);
		return Math.Floor(Convert.ToDouble(parameters[0].Evaluate(base.Variables)));
	}
}
