using System;
using System.Reflection;

namespace NGTools;

public static class UWPFakeExtension
{
	public static Assembly Assembly(this Type t)
	{
		return t.Assembly;
	}

	public static Type BaseType(this Type t)
	{
		return t.BaseType;
	}

	public static bool IsInterface(this Type t)
	{
		return t.IsInterface;
	}

	public static bool IsPrimitive(this Type t)
	{
		return t.IsPrimitive;
	}

	public static bool IsGenericType(this Type t)
	{
		return t.IsGenericType;
	}

	public static bool IsEnum(this Type t)
	{
		return t.IsEnum;
	}

	public static bool IsClass(this Type t)
	{
		return t.IsClass;
	}

	public static bool IsValueType(this Type t)
	{
		return t.IsValueType;
	}
}
