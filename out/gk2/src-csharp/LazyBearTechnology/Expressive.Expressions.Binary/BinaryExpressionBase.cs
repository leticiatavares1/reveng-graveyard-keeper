using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expressive.Exceptions;

namespace Expressive.Expressions.Binary;

public abstract class BinaryExpressionBase : IExpression
{
	private readonly IExpression leftHandSide;

	private readonly IExpression rightHandSide;

	protected Context Context { get; }

	protected BinaryExpressionBase(IExpression lhs, IExpression rhs, Context context)
	{
		leftHandSide = lhs;
		Context = context;
		rightHandSide = rhs;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		if (leftHandSide == null)
		{
			throw new MissingParticipantException("The left hand side of the operation is missing.");
		}
		if (rightHandSide == null)
		{
			throw new MissingParticipantException("The right hand side of the operation is missing.");
		}
		object lhsResult = leftHandSide.Evaluate(variables);
		return EvaluateImpl(lhsResult, rightHandSide, variables);
	}

	protected abstract object EvaluateImpl(object lhsResult, IExpression rightHandSide, IDictionary<string, object> variables);

	public static object EvaluateAggregates(object lhsResult, IExpression rhs, IDictionary<string, object> variables, Func<object, object, object> resultSelector)
	{
		if (rhs == null)
		{
			throw new ArgumentNullException("rhs");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		IList<object> list = new List<object>();
		IList<object> list2 = new List<object>();
		object obj = rhs.Evaluate(variables);
		if (!(lhsResult is ICollection) && !(obj is ICollection))
		{
			return resultSelector(lhsResult, obj);
		}
		if (lhsResult is ICollection collection)
		{
			foreach (object item in collection)
			{
				list.Add(item);
			}
		}
		if (obj is ICollection collection2)
		{
			foreach (object item2 in collection2)
			{
				list2.Add(item2);
			}
		}
		object[] result = null;
		if (list.Count == list2.Count)
		{
			IList<object> list3 = new List<object>();
			for (int i = 0; i < list.Count; i++)
			{
				list3.Add(resultSelector(list[i], list2[i]));
			}
			result = list3.ToArray();
		}
		else if (list.Count == 0)
		{
			IList<object> list4 = new List<object>();
			for (int j = 0; j < list2.Count; j++)
			{
				list4.Add(resultSelector(lhsResult, list2[j]));
			}
			result = list4.ToArray();
		}
		else if (list2.Count == 0)
		{
			IList<object> list5 = new List<object>();
			for (int k = 0; k < list.Count; k++)
			{
				list5.Add(resultSelector(list[k], obj));
			}
			result = list5.ToArray();
		}
		return result;
	}
}
