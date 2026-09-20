using System.Collections;
using System.Collections.Generic;
using Expressive.Expressions;
using LinqTools;

namespace Expressive.Functions.Statistical;

internal class ModeFunction : FunctionBase
{
	public override string Name => "Mode";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, -1, 1);
		IList<object> list = new List<object>();
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj = parameters[i].Evaluate(base.Variables);
			if (obj is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					list.Add(item);
				}
			}
			else
			{
				list.Add(obj);
			}
		}
		IEnumerable<IGrouping<object, object>> source = from v in list
			group v by v;
		int maxCount = source.Max((IGrouping<object, object> g) => g.Count());
		return source.First((IGrouping<object, object> g) => g.Count() == maxCount).Key;
	}
}
