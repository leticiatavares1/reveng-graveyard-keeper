using System;
using System.Diagnostics;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
[Conditional("BALANCE_PARSER")]
public class AutoValidate_StrListInCollectionAttribute : AutoValidateAttribute
{
	public Type TypeOfBalanceBaseObject { get; }

	public AutoValidate_StrListInCollectionAttribute(Type typeOfBalanceBaseObject)
	{
		TypeOfBalanceBaseObject = typeOfBalanceBaseObject;
	}
}
