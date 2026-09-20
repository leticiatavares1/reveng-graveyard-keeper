using System;
using System.Collections;
using Expressive.Expressions;
using Expressive.Helpers;

namespace Expressive.Functions.Statistical;

internal class AverageFunction : FunctionBase
{
	public override string Name => "Average";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, -1, 1);
		int num = 0;
		object obj = 0;
		foreach (IExpression obj2 in parameters)
		{
			int num2 = 1;
			object obj3 = obj2.Evaluate(base.Variables);
			if (obj3 is IEnumerable enumerable)
			{
				int num3 = 0;
				object obj4 = 0;
				foreach (object item in enumerable)
				{
					num3++;
					obj4 = Numbers.Add(obj4, item);
				}
				num2 = num3;
				obj3 = obj4;
			}
			obj = Numbers.Add(obj, obj3);
			num += num2;
		}
		return Convert.ToDouble(obj) / (double)num;
	}
}
