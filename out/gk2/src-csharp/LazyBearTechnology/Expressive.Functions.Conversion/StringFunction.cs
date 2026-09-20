using Expressive.Expressions;

namespace Expressive.Functions.Conversion;

internal sealed class StringFunction : FunctionBase
{
	public override string Name => "String";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		if (parameters.Length > 1 && parameters[1].Evaluate(base.Variables) is string text)
		{
			return string.Format(context.CurrentCulture, "{0:" + text + "}", obj);
		}
		return obj.ToString();
	}
}
