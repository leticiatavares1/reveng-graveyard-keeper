using System;
using System.Diagnostics;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
[Conditional("UNITY_EDITOR")]
[Conditional("BALANCE_PARSER")]
public abstract class AutoValidateAttribute : Attribute
{
}
