using System;

namespace ParadoxNotion.Design;

[AttributeUsage(AttributeTargets.Field)]
public class CallbackAttribute : Attribute
{
	public string methodName;

	public CallbackAttribute(string methodName)
	{
		this.methodName = methodName;
	}
}
