using System;
using System.Reflection;

namespace NGTools;

public class FieldModifier : IFieldModifier, IValueGetter
{
	private readonly FieldInfo fieldInfo;

	public Type Type => fieldInfo.FieldType;

	public string Name => fieldInfo.Name;

	public bool IsPublic => fieldInfo.IsPublic;

	protected FieldModifier()
	{
	}

	public FieldModifier(FieldInfo fieldInfo)
	{
		this.fieldInfo = fieldInfo;
	}

	public void SetValue(object instance, object value)
	{
		fieldInfo.SetValue(instance, value);
	}

	public object GetValue(object instance)
	{
		return fieldInfo.GetValue(instance);
	}

	public T GetValue<T>(object instance)
	{
		return (T)fieldInfo.GetValue(instance);
	}

	public bool IsDefined(Type type, bool inherit)
	{
		return fieldInfo.IsDefined(type, inherit);
	}

	public object[] GetCustomAttributes(Type type, bool inherit)
	{
		return fieldInfo.GetCustomAttributes(type, inherit);
	}
}
