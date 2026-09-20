using System;
using ParadoxNotion;

namespace FlowCanvas;

public abstract class ValueOutput : Port
{
	public ValueOutput()
	{
	}

	public ValueOutput(FlowNode parent, string name, string ID)
		: base(parent, name, ID)
	{
	}

	public static ValueOutput<T> CreateInstance<T>(FlowNode parent, string name, string ID, ValueHandler<T> getter)
	{
		return new ValueOutput<T>(parent, name, ID, getter);
	}

	public static ValueOutput CreateInstance(Type t, FlowNode parent, string name, string ID, ValueHandlerObject getter)
	{
		return (ValueOutput)Activator.CreateInstance(typeof(ValueOutput<>).RTMakeGenericType(t), parent, name, ID, getter);
	}

	public abstract object GetValue();
}
public class ValueOutput<T> : ValueOutput
{
	public ValueHandler<T> getter { get; private set; }

	public override Type type => typeof(T);

	public ValueOutput()
	{
	}

	public ValueOutput(FlowNode parent, string name, string ID, ValueHandler<T> getter)
		: base(parent, name, ID)
	{
		this.getter = getter;
	}

	public ValueOutput(FlowNode parent, string name, string ID, ValueHandlerObject getter)
		: base(parent, name, ID)
	{
		this.getter = () => (T)getter();
	}

	public override object GetValue()
	{
		return getter();
	}

	public static explicit operator T(ValueOutput<T> port)
	{
		return port.getter();
	}
}
