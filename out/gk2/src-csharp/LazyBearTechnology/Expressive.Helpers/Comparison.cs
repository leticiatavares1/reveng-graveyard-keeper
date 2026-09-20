using System;
using System.Collections.Generic;

namespace Expressive.Helpers;

public static class Comparison
{
	private static readonly Type[] CommonTypes = new Type[7]
	{
		typeof(DateTime),
		typeof(decimal),
		typeof(double),
		typeof(long),
		typeof(int),
		typeof(bool),
		typeof(string)
	};

	public static int CompareUsingMostPreciseType(object lhs, object rhs, Context context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		Type mostPreciseType = GetMostPreciseType(lhs?.GetType(), rhs?.GetType());
		if (mostPreciseType == typeof(string))
		{
			return string.Compare((string)Convert.ChangeType(lhs, mostPreciseType, context.CurrentCulture), (string)Convert.ChangeType(rhs, mostPreciseType, context.CurrentCulture), context.EqualityStringComparison);
		}
		return Compare(lhs, rhs, mostPreciseType, context);
	}

	private static Type GetMostPreciseType(Type a, Type b)
	{
		if (a == b)
		{
			return a;
		}
		Type[] commonTypes = CommonTypes;
		foreach (Type type in commonTypes)
		{
			if (a == type || b == type)
			{
				return type;
			}
		}
		return a;
	}

	private static int Compare(object lhs, object rhs, Type mostPreciseType, Context context)
	{
		if (lhs == null && rhs == null)
		{
			return 0;
		}
		if (lhs == null)
		{
			return -1;
		}
		if (rhs == null)
		{
			return 1;
		}
		Type type = lhs.GetType();
		Type type2 = rhs.GetType();
		if (type == type2)
		{
			return Comparer<object>.Default.Compare(lhs, rhs);
		}
		try
		{
			if (type == mostPreciseType)
			{
				rhs = Convert.ChangeType(rhs, mostPreciseType, context.CurrentCulture);
			}
			else
			{
				lhs = Convert.ChangeType(lhs, mostPreciseType, context.CurrentCulture);
			}
			return Comparer<object>.Default.Compare(lhs, rhs);
		}
		catch (Exception)
		{
		}
		try
		{
			return Comparer<object>.Default.Compare(lhs, Convert.ChangeType(rhs, type, context.CurrentCulture));
		}
		catch (Exception)
		{
		}
		try
		{
			return Comparer<object>.Default.Compare(lhs, Convert.ChangeType(rhs, type, context.CurrentCulture));
		}
		catch (Exception)
		{
		}
		try
		{
			return string.Compare((string)Convert.ChangeType(lhs, typeof(string), context.CurrentCulture), (string)Convert.ChangeType(rhs, typeof(string), context.CurrentCulture), context.EqualityStringComparison);
		}
		catch (Exception)
		{
		}
		return 0;
	}
}
