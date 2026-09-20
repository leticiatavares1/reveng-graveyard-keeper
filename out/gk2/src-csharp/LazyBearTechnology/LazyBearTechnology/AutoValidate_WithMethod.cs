using System;
using System.Diagnostics;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
[Conditional("BALANCE_PARSER")]
public class AutoValidate_WithMethod : AutoValidateAttribute
{
	public string MethodName { get; }

	public AutoValidate_WithMethod(string methodName)
	{
		MethodName = methodName;
	}
}
