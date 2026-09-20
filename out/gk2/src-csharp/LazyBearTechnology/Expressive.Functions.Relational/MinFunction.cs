using System.Collections;
using System.Linq;
using Expressive.Expressions;
using Expressive.Helpers;

namespace Expressive.Functions.Relational;

internal class MinFunction : FunctionBase
{
	public override string Name => "Min";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj is IEnumerable enumerable)
		{
			obj = Min(enumerable, context);
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
				obj2 = Min(enumerable2, context);
			}
			obj = ((Comparison.CompareUsingMostPreciseType(obj, obj2, context) < 0) ? obj : obj2);
			if (obj == null)
			{
				return null;
			}
		}
		return obj;
	}

	private static object Min(IEnumerable enumerable, Context context)
	{
		object obj = null;
		foreach (object item in enumerable)
		{
			if (item == null)
			{
				return null;
			}
			obj = ((obj != null) ? ((Comparison.CompareUsingMostPreciseType(obj, item, context) < 0) ? obj : item) : item);
		}
		return obj;
	}
}
