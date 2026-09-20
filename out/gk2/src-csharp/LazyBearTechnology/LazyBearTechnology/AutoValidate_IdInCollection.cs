using System;
using System.Diagnostics;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
[Conditional("BALANCE_PARSER")]
public class AutoValidate_IdInCollection : AutoValidateAttribute
{
	public Type TypeOfBalanceBaseObject { get; }

	public AutoValidate_IdInCollection(Type typeOfBalanceBaseObject)
	{
		TypeOfBalanceBaseObject = typeOfBalanceBaseObject;
	}
}
