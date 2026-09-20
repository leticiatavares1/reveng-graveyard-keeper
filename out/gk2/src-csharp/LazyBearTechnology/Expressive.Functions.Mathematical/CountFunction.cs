using System.Collections;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class CountFunction : FunctionBase
{
	public override string Name => "Count";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		int num = 0;
		foreach (IExpression obj in parameters)
		{
			int num2 = 1;
			if (obj.Evaluate(base.Variables) is ICollection collection)
			{
				num2 = collection.Count;
			}
			num += num2;
		}
		return num;
	}
}
