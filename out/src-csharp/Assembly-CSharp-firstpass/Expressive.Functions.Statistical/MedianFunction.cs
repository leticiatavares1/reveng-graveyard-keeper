using System;
using System.Collections;
using System.Collections.Generic;
using Expressive.Expressions;
using LinqTools;

namespace Expressive.Functions.Statistical;

internal class MedianFunction : FunctionBase
{
	public override string Name => "Median";

	public override object Evaluate(IExpression[] parameters)
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
					list.Add(Convert.ToDecimal(item));
				}
			}
			else
			{
				list.Add(Convert.ToDecimal(obj));
			}
		}
		return Median(list.ToArray());
	}

	private decimal Median(decimal[] xs)
	{
		List<decimal> list = xs.OrderBy((decimal x) => x).ToList();
		double num = (double)(list.Count - 1) / 2.0;
		return (list[(int)num] + list[(int)(num + 0.5)]) / 2m;
	}
}
