using System.Collections;
using System.Linq;
using Expressive.Expressions;
using Expressive.Helpers;

namespace Expressive.Functions.Logical;

internal class InFunction : FunctionBase
{
	public override string Name => "In";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 2);
		bool flag = false;
		object parameter = parameters[0].Evaluate(base.Variables);
		for (int i = 1; i < parameters.Length; i++)
		{
			object obj = parameters[i].Evaluate(base.Variables);
			if (obj is ICollection source)
			{
				if (source.Cast<object>().Any((object innerValue) => Comparison.CompareUsingMostPreciseType(parameter, innerValue, context) == 0))
				{
					flag = true;
					break;
				}
			}
			else
			{
				flag = Comparison.CompareUsingMostPreciseType(parameter, obj, context) == 0;
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}
}
