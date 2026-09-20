using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class NetworkMethodAttribute : Attribute
{
	public Type TargetType { get; }

	public string MethodName { get; }

	public object[] Parameters { get; }

	public NetworkMethodAttribute(Type targetType, string methodName, params object[] parameters)
	{
		TargetType = targetType;
		MethodName = methodName;
		Parameters = parameters;
	}
}
