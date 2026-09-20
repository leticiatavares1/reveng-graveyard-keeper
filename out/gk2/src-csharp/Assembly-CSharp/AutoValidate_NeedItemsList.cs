using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
[Conditional("BALANCE_PARSER")]
public class AutoValidate_NeedItemsList : AutoValidate_NeedItem
{
}
