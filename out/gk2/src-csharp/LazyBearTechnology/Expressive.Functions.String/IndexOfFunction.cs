using System;
using System.Collections;
using Expressive.Expressions;

namespace Expressive.Functions.String;

internal class IndexOfFunction : FunctionBase
{
	public override string Name => "IndexOf";

	public override object Evaluate(IExpression[] parameters, Context context)
	{
		ValidateParameterCount(parameters, -1, 2);
		object obj = parameters[0].Evaluate(base.Variables);
		if (obj == null)
		{
			return null;
		}
		if (obj is string)
		{
			string text = obj.ToString();
			obj = parameters[1].Evaluate(base.Variables);
			if (obj == null)
			{
				return null;
			}
			string value = obj.ToString();
			if (parameters.Length > 2)
			{
				int startIndex = Convert.ToInt32(parameters[2].Evaluate(base.Variables));
				return text.IndexOf(value, startIndex, context.EqualityStringComparison);
			}
			return text.IndexOf(value, context.EqualityStringComparison);
		}
		if (obj is IEnumerable enumerable)
		{
			int num = 0;
			obj = parameters[1].Evaluate(base.Variables);
			foreach (object item in enumerable)
			{
				object obj2 = item;
				if (obj2 is IExpression)
				{
					obj2 = (obj2 as IExpression).Evaluate(base.Variables);
				}
				if (obj2 != null)
				{
					if (obj2 is string text2)
					{
						if (obj is string value2 && text2.Equals(value2, context.EqualityStringComparison))
						{
							return num;
						}
					}
					else if (obj.Equals(obj2))
					{
						return num;
					}
				}
				num++;
			}
			return -1;
		}
		if (obj is IComparable)
		{
			IComparable obj3 = obj as IComparable;
			obj = parameters[1].Evaluate(base.Variables);
			if (obj3.CompareTo(obj) == 0)
			{
				return 0;
			}
			return -1;
		}
		return -1;
	}
}
