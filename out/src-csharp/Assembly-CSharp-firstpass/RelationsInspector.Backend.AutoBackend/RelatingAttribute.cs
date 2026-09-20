using System;

namespace RelationsInspector.Backend.AutoBackend;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RelatingAttribute : Attribute
{
}
