using System;
using System.Reflection;

namespace NGTools;

public class PropertyModifier : IFieldModifier, IValueGetter
{
	private readonly PropertyInfo propertyInfo;

	public Type Type => propertyInfo.PropertyType;

	public string Name => propertyInfo.Name;

	public bool IsPublic => propertyInfo.CanRead;

	public PropertyModifier(PropertyInfo propertyInfo)
	{
		this.propertyInfo = propertyInfo;
	}

	public void SetValue(object instance, object value)
	{
		propertyInfo.SetValue(instance, value, null);
	}

	public object GetValue(object instance)
	{
		return propertyInfo.GetValue(instance, null);
	}

	public T GetValue<T>(object instance)
	{
		return (T)propertyInfo.GetValue(instance, null);
	}

	public bool IsDefined(Type type, bool inherit)
	{
		return propertyInfo.IsDefined(type, inherit);
	}

	public object[] GetCustomAttributes(Type type, bool inherit)
	{
		return propertyInfo.GetCustomAttributes(type, inherit);
	}
}
