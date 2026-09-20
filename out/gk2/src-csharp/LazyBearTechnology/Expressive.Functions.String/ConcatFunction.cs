using System.Collections;
using System.Text;
using Expressive.Expressions;

namespace Expressive.Functions.String;

internal class ConcatFunction : FunctionBase
{
	public override string Name => "Concat";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		StringBuilder stringBuilder = new StringBuilder();
		Evaluate(stringBuilder, parameters, context);
		return stringBuilder.ToString();
	}

	protected virtual void Evaluate(StringBuilder sb, IEnumerable parameters, Context context)
	{
		foreach (object parameter in parameters)
		{
			object obj = ((!(parameter is IExpression expression)) ? parameter : expression.Evaluate(base.Variables));
			if (obj != null)
			{
				if (obj is string)
				{
					sb.Append(obj);
					continue;
				}
				if (obj is IEnumerable parameters2)
				{
					Evaluate(sb, parameters2, context);
					continue;
				}
				string value = obj.ToString();
				sb.Append(value);
			}
		}
	}
}
