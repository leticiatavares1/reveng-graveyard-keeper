using System;
using System.Diagnostics;
using LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
[Conditional("BALANCE_PARSER")]
public class AutoValidate_OutputItems : AutoValidateAttribute
{
}
