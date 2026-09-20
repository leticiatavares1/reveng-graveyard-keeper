using System;
using System.Collections;

namespace Expressive.Helpers;

internal static class Comparison
{
	private static Type[] CommonTypes = new Type[6]
	{
		typeof(long),
		typeof(double),
		typeof(bool),
		typeof(DateTime),
		typeof(string),
		typeof(decimal)
	};

	internal static int CompareUsingMostPreciseType(object a, object b)
	{
		Type mostPreciseType = GetMostPreciseType(a.GetType(), b.GetType());
		return Comparer.Default.Compare(Convert.ChangeType(a, mostPreciseType), Convert.ChangeType(b, mostPreciseType));
	}

	internal static Type GetMostPreciseType(Type a, Type b)
	{
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
}
