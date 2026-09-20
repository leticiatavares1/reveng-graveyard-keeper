using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class PIFunction : FunctionBase
{
	public override string Name => "PI";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 0, 0);
		return Math.PI;
	}
}
