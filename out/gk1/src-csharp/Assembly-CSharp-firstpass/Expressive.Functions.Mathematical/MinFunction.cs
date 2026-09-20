using System.Collections;
using Expressive.Expressions;
using Expressive.Helpers;
using LinqTools;

namespace Expressive.Functions.Mathematical;

internal class MinFunction : FunctionBase
{
	public override string Name => "Min";

	public override object Evaluate(IExpression[] parameters)
	{
		ValidateParameterCount(parameters, -1, 1);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj is IEnumerable)
		{
			obj = Min((IEnumerable)obj);
		}
		if (obj == null)
		{
			return null;
		}
		foreach (IExpression item in parameters.Skip(1))
		{
			object obj2 = item.Evaluate(base.Variables);
			if (obj2 is IEnumerable enumerable)
			{
				obj2 = Min(enumerable);
			}
			obj = Numbers.Min(obj, obj2);
			if (obj == null)
			{
				return null;
			}
		}
		return obj;
	}

	private object Min(IEnumerable enumerable)
	{
		object obj = null;
		foreach (object item in enumerable)
		{
			if (item == null)
			{
				return null;
			}
			obj = ((obj != null) ? Numbers.Min(obj, item) : item);
		}
		return obj;
	}
}
