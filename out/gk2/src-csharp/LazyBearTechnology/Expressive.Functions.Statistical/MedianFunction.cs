using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expressive.Expressions;

namespace Expressive.Functions.Statistical;

internal class MedianFunction : FunctionBase
{
	public override string Name => "Median";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		IList<decimal> list = new List<decimal>();
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj = parameters[i].Evaluate(base.Variables);
			if (obj is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					AddValue(item, list);
				}
			}
			else
			{
				AddValue(obj, list);
			}
		}
		return Median(list.ToArray());
	}

	private static void AddValue(object value, IList<decimal> decimalValues)
	{
		if (value != null)
		{
			decimalValues.Add(Convert.ToDecimal(value));
		}
	}

	private static decimal Median(IEnumerable<decimal> xs)
	{
		List<decimal> list = xs.OrderBy((decimal x) => x).ToList();
		double num = (double)(list.Count - 1) / 2.0;
		return (list[(int)num] + list[(int)(num + 0.5)]) / 2m;
	}
}
