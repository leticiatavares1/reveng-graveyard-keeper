using Expressive.Expressions;

namespace Expressive.Functions.Statistical;

internal class AverageFunction : FunctionBase
{
	public override string Name => "Average";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		return MeanFunction.Evaluate(parameters, base.Variables);
	}
}
