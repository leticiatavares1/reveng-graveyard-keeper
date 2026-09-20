using System;
using System.Reflection;

public class InterceptorInfo
{
	public Type TargetType { get; }

	public MethodInfo TargetMethod { get; }

	public Type BaseGenericArgumentType { get; }

	public InterceptorInfo(Type targetType, MethodInfo targetMethod, Type baseGenericArgumentType)
	{
		TargetType = targetType;
		TargetMethod = targetMethod;
		BaseGenericArgumentType = baseGenericArgumentType;
	}
}
