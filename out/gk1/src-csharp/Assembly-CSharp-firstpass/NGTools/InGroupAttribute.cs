using System;

namespace NGTools;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InGroupAttribute : Attribute
{
	public readonly string group;

	public InGroupAttribute(string group)
	{
		this.group = group;
	}
}
