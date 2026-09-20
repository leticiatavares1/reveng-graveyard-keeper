using System.Collections;
using Expressive.Expressions;
using Expressive.Helpers;

namespace Expressive.Functions.Mathematical;

internal class SumFunction : FunctionBase
{
	public override string Name => "Sum";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 1);
		object obj = 0;
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj2 = parameters[i].Evaluate(base.Variables);
			if (obj2 is IEnumerable enumerable)
			{
				object obj3 = 0;
				foreach (object item in enumerable)
				{
					obj3 = Numbers.Add(obj3 ?? ((object)0), item ?? ((object)0));
				}
				obj2 = obj3;
			}
			obj = Numbers.Add(obj ?? ((object)0), obj2 ?? ((object)0));
		}
		return obj;
	}
}
