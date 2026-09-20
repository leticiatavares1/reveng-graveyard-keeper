using System;

namespace LazyBearTechnology;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class AutoParseAttribute : Attribute
{
	public string Column { get; }

	public object DefaultValue { get; }

	public AutoParseAttribute(string column)
	{
		Column = column;
		DefaultValue = null;
	}
}
