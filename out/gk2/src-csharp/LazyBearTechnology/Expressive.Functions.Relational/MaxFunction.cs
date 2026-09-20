using System.Collections;
using System.Linq;
using Expressive.Expressions;
using Expressive.Helpers;

namespace Expressive.Functions.Relational;

internal class MaxFunction : FunctionBase
{
	public override string Name => "Max";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj is IEnumerable enumerable)
		{
			obj = Max(enumerable, context);
		}
		if (obj == null)
		{
			return null;
		}
		foreach (IExpression item in parameters.Skip(1))
		{
			object obj2 = item.Evaluate(base.Variables);
			if (obj2 is IEnumerable enumerable2)
			{
				obj2 = Max(enumerable2, context);
			}
			if (obj2 == null)
			{
				return null;
			}
			obj = ((Comparison.CompareUsingMostPreciseType(obj, obj2, context) > 0) ? obj : obj2);
		}
		return obj;
	}

	private static object Max(IEnumerable enumerable, Context context)
	{
		object obj = null;
		foreach (object item in enumerable)
		{
			if (item == null)
			{
				return null;
			}
			obj = ((obj != null) ? ((Comparison.CompareUsingMostPreciseType(obj, item, context) > 0) ? obj : item) : item);
		}
		return obj;
	}
}
