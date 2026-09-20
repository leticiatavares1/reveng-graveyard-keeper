using System;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class LazyExpressionPureValueTypeAttribute : Attribute
{
	public PureValueType PureValueType { get; }

	public LazyExpressionPureValueTypeAttribute(PureValueType pureValueType)
	{
		PureValueType = pureValueType;
	}
}
