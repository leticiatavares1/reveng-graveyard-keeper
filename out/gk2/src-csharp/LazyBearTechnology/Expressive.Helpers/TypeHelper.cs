using System;

namespace Expressive.Helpers;

public static class TypeHelper
{
	public static TypeCode GetTypeCode(object value)
	{
		return Type.GetTypeCode(value?.GetType());
	}
}
