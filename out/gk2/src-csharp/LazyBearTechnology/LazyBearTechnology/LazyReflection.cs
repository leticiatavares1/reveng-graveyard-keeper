using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LazyBearTechnology;

public static class LazyReflection
{
	public static List<Type> GetAllTypes()
	{
		List<Type> list = new List<Type>();
		foreach (Assembly item in from a in AppDomain.CurrentDomain.GetAssemblies()
			where a.FullName.StartsWith("Assembly-CSharp")
			select a)
		{
			list.AddRange(item.GetTypes());
		}
		return list;
	}

	public static List<Type> GetAllDerivedClasses<T>() where T : class
	{
		Type baseType = typeof(T);
		return (from t in GetAllTypes()
			where t != baseType && baseType.IsAssignableFrom(t)
			select t).ToList();
	}

	public static List<MethodInfo> GetAllMethodsWithAttribute<T>(BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) where T : Attribute
	{
		List<MethodInfo> list = new List<MethodInfo>();
		foreach (Type allType in GetAllTypes())
		{
			MethodInfo[] methods = allType.GetMethods(flags);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.GetCustomAttributes(typeof(LazyUITestAttribute), inherit: false).Any())
				{
					list.Add(methodInfo);
				}
			}
		}
		return list;
	}
}
