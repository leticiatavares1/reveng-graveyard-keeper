using System;
using System.Collections;
using System.Collections.Generic;
using Expressive.Exceptions;
using Expressive.Helpers;
using LinqTools;

namespace Expressive.Expressions;

internal class BinaryExpression : IExpression
{
	private readonly BinaryExpressionType _expressionType;

	private readonly IExpression _leftHandSide;

	private readonly IExpression _rightHandSide;

	internal BinaryExpression(BinaryExpressionType type, IExpression lhs, IExpression rhs)
	{
		_expressionType = type;
		_leftHandSide = lhs;
		_rightHandSide = rhs;
	}

	public object Evaluate(IDictionary<string, object> variables)
	{
		if (_leftHandSide == null)
		{
			throw new MissingParticipantException("The left hand side of the operation is missing.");
		}
		if (_rightHandSide == null)
		{
			throw new MissingParticipantException("The right hand side of the operation is missing.");
		}
		object obj = _leftHandSide.Evaluate(variables);
		switch (_expressionType)
		{
		case BinaryExpressionType.And:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToBoolean(l) && Convert.ToBoolean(r));
		case BinaryExpressionType.Or:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToBoolean(l) || Convert.ToBoolean(r));
		case BinaryExpressionType.NotEqual:
		{
			object obj3 = null;
			if (obj == null)
			{
				obj3 = _rightHandSide.Evaluate(variables);
				if (obj3 != null)
				{
					return true;
				}
				return false;
			}
			obj3 = _rightHandSide.Evaluate(variables);
			if (obj3 == null)
			{
				return true;
			}
			return Comparison.CompareUsingMostPreciseType(obj, obj3) != 0;
		}
		case BinaryExpressionType.LessThanOrEqual:
		{
			if (obj == null)
			{
				return null;
			}
			object obj6 = _rightHandSide.Evaluate(variables);
			if (obj6 == null)
			{
				return null;
			}
			return Comparison.CompareUsingMostPreciseType(obj, obj6) <= 0;
		}
		case BinaryExpressionType.GreaterThanOrEqual:
		{
			if (obj == null)
			{
				return null;
			}
			object obj7 = _rightHandSide.Evaluate(variables);
			if (obj7 == null)
			{
				return null;
			}
			return Comparison.CompareUsingMostPreciseType(obj, obj7) >= 0;
		}
		case BinaryExpressionType.LessThan:
		{
			if (obj == null)
			{
				return null;
			}
			object obj4 = _rightHandSide.Evaluate(variables);
			if (obj4 == null)
			{
				return null;
			}
			return Comparison.CompareUsingMostPreciseType(obj, obj4) < 0;
		}
		case BinaryExpressionType.GreaterThan:
		{
			if (obj == null)
			{
				return null;
			}
			object obj5 = _rightHandSide.Evaluate(variables);
			if (obj5 == null)
			{
				return null;
			}
			return Comparison.CompareUsingMostPreciseType(obj, obj5) > 0;
		}
		case BinaryExpressionType.Equal:
		{
			object obj2 = null;
			if (obj == null)
			{
				obj2 = _rightHandSide.Evaluate(variables);
				if (obj2 == null)
				{
					return true;
				}
				return false;
			}
			obj2 = _rightHandSide.Evaluate(variables);
			if (obj2 == null)
			{
				return false;
			}
			return Comparison.CompareUsingMostPreciseType(obj, obj2) == 0;
		}
		case BinaryExpressionType.Subtract:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Numbers.Subtract(l, r));
		case BinaryExpressionType.Add:
			if (obj is string)
			{
				return (string)obj + _rightHandSide.Evaluate(variables);
			}
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Numbers.Add(l, r));
		case BinaryExpressionType.Modulus:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Numbers.Modulus(l, r));
		case BinaryExpressionType.Divide:
			_rightHandSide.Evaluate(variables);
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => (l != null && r != null && !IsReal(l) && !IsReal(r)) ? Numbers.Divide(Convert.ToDouble(l), r) : Numbers.Divide(l, r));
		case BinaryExpressionType.Multiply:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Numbers.Multiply(l, r));
		case BinaryExpressionType.BitwiseOr:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToUInt16(l) | Convert.ToUInt16(r));
		case BinaryExpressionType.BitwiseAnd:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToUInt16(l) & Convert.ToUInt16(r));
		case BinaryExpressionType.BitwiseXOr:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToUInt16(l) ^ Convert.ToUInt16(r));
		case BinaryExpressionType.LeftShift:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToUInt16(l) << (int)Convert.ToUInt16(r));
		case BinaryExpressionType.RightShift:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => Convert.ToUInt16(l) >> (int)Convert.ToUInt16(r));
		case BinaryExpressionType.NullCoalescing:
			return Evaluate(obj, _rightHandSide, variables, (object l, object r) => l ?? r);
		default:
			return null;
		}
	}

	private static bool IsReal(object value)
	{
		TypeCode typeCode = ReflectionTools.GetTypeCode(value);
		if (typeCode != TypeCode.Decimal && typeCode != TypeCode.Double)
		{
			return typeCode == TypeCode.Single;
		}
		return true;
	}

	private object Evaluate(object lhsResult, IExpression rhs, IDictionary<string, object> variables, Func<object, object, object> resultSelector)
	{
		IList<object> list = new List<object>();
		IList<object> list2 = new List<object>();
		object obj = rhs.Evaluate(variables);
		if (!(lhsResult is IEnumerable) && !(obj is IEnumerable))
		{
			return resultSelector(lhsResult, obj);
		}
		if (lhsResult is IEnumerable)
		{
			foreach (object item in (IEnumerable)lhsResult)
			{
				list.Add(item);
			}
		}
		if (obj is IEnumerable)
		{
			foreach (object item2 in (IEnumerable)obj)
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
