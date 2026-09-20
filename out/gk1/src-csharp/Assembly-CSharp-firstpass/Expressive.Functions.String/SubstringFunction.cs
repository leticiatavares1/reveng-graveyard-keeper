using Expressive.Expressions;

namespace Expressive.Functions.String;

internal class SubstringFunction : FunctionBase
{
	public override string Name => "Substring";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, 3, 3);
		string obj = (string)parameters[0].Evaluate(base.Variables);
		int startIndex = (int)parameters[1].Evaluate(base.Variables);
		int length = (int)parameters[2].Evaluate(base.Variables);
		return obj.Substring(startIndex, length);
	}
}
