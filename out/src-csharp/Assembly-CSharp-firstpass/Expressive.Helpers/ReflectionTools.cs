using System;

namespace Expressive.Helpers;

internal static class ReflectionTools
{
	public static TypeCode GetTypeCode(object value)
	{
		return Type.GetTypeCode(value.GetType());
	}
}
