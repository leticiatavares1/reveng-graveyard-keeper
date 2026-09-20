using System;
using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas;

[SpoofAOT]
public struct Flow
{
	public int ticks;

	public Dictionary<string, object> parameters;

	public FlowBreak Break;

	public FlowReturn Return;

	public Type ReturnType;

	public static Flow New => default(Flow);

	public T ReadParameter<T>(string name)
	{
		object value = default(T);
		if (parameters != null)
		{
			parameters.TryGetValue(name, out value);
		}
		if (!(value is T))
		{
			return default(T);
		}
		return (T)value;
	}

	public void WriteParameter<T>(string name, T value)
	{
		if (parameters == null)
		{
			parameters = new Dictionary<string, object>();
		}
		parameters[name] = value;
	}
}
