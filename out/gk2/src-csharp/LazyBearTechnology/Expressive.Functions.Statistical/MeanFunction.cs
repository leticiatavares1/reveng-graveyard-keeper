using System;
using System.Collections;
using System.Collections.Generic;
using Expressive.Expressions;
using Expressive.Helpers;

namespace Expressive.Functions.Statistical;

internal class MeanFunction : FunctionBase
{
	public override string Name => "Mean";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		return Evaluate(parameters, base.Variables);
	}

	internal static object Evaluate(IExpression[] parameters, IDictionary<string, object> variables)
	{
		int num = 0;
		object obj = 0;
		foreach (IExpression obj2 in parameters)
		{
			int num2 = 1;
			object obj3 = obj2.Evaluate(variables);
			if (obj3 is IEnumerable enumerable)
			{
				int num3 = 0;
				object obj4 = 0;
				foreach (object item in enumerable)
				{
					if (item != null)
					{
						num3++;
						obj4 = Numbers.Add(obj4, item);
					}
				}
				num2 = num3;
				obj3 = obj4;
			}
			else if (obj3 == null)
			{
				continue;
			}
			obj = Numbers.Add(obj, obj3);
			num += num2;
		}
		return Convert.ToDouble(obj) / (double)num;
	}
}
