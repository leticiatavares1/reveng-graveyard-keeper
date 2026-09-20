using System;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class EFunction : FunctionBase
{
	public override string Name => "E";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 0, 0);
		return Math.E;
	}
}
