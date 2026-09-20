using Expressive.Expressions;

namespace Expressive.Functions.String;

internal class StartsWithFunction : FunctionBase
{
	public override string Name => "StartsWith";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 2, 2);
		string text = (string)parameters[0].Evaluate(base.Variables);
		string text2 = (string)parameters[1].Evaluate(base.Variables);
		if (text2 == null)
		{
			return false;
		}
		return text?.StartsWith(text2, context.EqualityStringComparison) ?? false;
	}
}
