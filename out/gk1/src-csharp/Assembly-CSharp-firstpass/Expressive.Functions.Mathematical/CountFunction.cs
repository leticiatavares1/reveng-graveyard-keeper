using System.Collections;
using Expressive.Expressions;

namespace Expressive.Functions.Mathematical;

internal class CountFunction : FunctionBase
{
	public override string Name => "Count";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, -1, 1);
		int num = 0;
		foreach (IExpression obj in parameters)
		{
			int num2 = 1;
			if (obj.Evaluate(base.Variables) is IEnumerable enumerable)
			{
				int num3 = 0;
				foreach (object item in enumerable)
				{
					_ = item;
					num3++;
				}
				num2 = num3;
			}
			num += num2;
		}
		return num;
	}
}
