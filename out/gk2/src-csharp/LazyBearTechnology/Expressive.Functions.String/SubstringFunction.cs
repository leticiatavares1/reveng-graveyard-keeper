using Expressive.Expressions;

namespace Expressive.Functions.String;

internal class SubstringFunction : FunctionBase
{
	public override string Name => "Substring";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, 3, 3);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		string text = null;
		text = ((!(obj is string)) ? obj.ToString() : ((string)obj));
		int startIndex = (int)parameters[1].Evaluate(base.Variables);
		int length = (int)parameters[2].Evaluate(base.Variables);
		return text?.Substring(startIndex, length);
	}
}
