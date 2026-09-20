using System;
using System.Collections.Generic;
using System.Reflection;

public static class InterceptorCache
{
	private static readonly Dictionary<MethodInfo, InterceptorInfo> cache;

	static InterceptorCache()
	{
		cache = new Dictionary<MethodInfo, InterceptorInfo>();
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			MethodInfo[] methods = types[i].GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				NetworkMethodAttribute customAttribute = methodInfo.GetCustomAttribute<NetworkMethodAttribute>();
				if (customAttribute == null)
				{
					continue;
				}
				MethodInfo method = customAttribute.TargetType.GetMethod(customAttribute.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					Type[] baseGenericTypeArguments = GetBaseGenericTypeArguments(customAttribute.TargetType, typeof(CommandTargeted<>));
					if (baseGenericTypeArguments.Length > 1)
					{
						throw new ArgumentException($"Unsupported having more than 1 generic arguments for type [{customAttribute.TargetType}]");
					}
					Type baseGenericArgumentType = ((baseGenericTypeArguments.Length == 1) ? baseGenericTypeArguments[0] : null);
					cache[methodInfo] = new InterceptorInfo(customAttribute.TargetType, method, baseGenericArgumentType);
				}
			}
		}
	}

	public static InterceptorInfo GetInterceptorInfo(MethodInfo method)
	{
		if (!cache.TryGetValue(method, out var value))
		{
			return null;
		}
		return value;
	}

	private static Type[] GetBaseGenericTypeArguments(Type derivedType, Type baseGenericType)
	{
		while (derivedType != null && derivedType != typeof(object))
		{
			if ((derivedType.IsGenericType ? derivedType.GetGenericTypeDefinition() : derivedType) == baseGenericType)
			{
				return derivedType.GetGenericArguments();
			}
			derivedType = derivedType.BaseType;
		}
		return null;
	}
}
