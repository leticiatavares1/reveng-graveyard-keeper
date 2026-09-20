using System;
using ParadoxNotion;

namespace FlowCanvas;

public abstract class ValueInput : Port
{
	public object value => GetValue();

	public abstract object serializedValue { get; set; }

	public abstract bool isDefaultValue { get; }

	public abstract override Type type { get; }

	public ValueInput()
	{
	}

	public ValueInput(FlowNode parent, string name, string ID)
		: base(parent, name, ID)
	{
	}

	public static ValueInput<T> CreateInstance<T>(FlowNode parent, string name, string ID)
	{
		return new ValueInput<T>(parent, name, ID);
	}

	public static ValueInput CreateInstance(Type t, FlowNode parent, string name, string ID)
	{
		return (ValueInput)Activator.CreateInstance(typeof(ValueInput<>).RTMakeGenericType(t), parent, name, ID);
	}

	public abstract void BindTo(ValueOutput target);

	public abstract void UnBind();

	public abstract object GetValue();
}
public class ValueInput<T> : ValueInput
{
	private T _value;

	public ValueHandler<T> getter { get; private set; }

	public new T value
	{
		get
		{
			if (getter != null)
			{
				return getter();
			}
			return _value;
		}
	}

	public override bool isDefaultValue => object.Equals(_value, default(T));

	public override object serializedValue
	{
		get
		{
			return _value;
		}
		set
		{
			_value = (T)value;
		}
	}

	public override Type type => typeof(T);

	public ValueInput()
	{
	}

	public ValueInput(FlowNode parent, string name, string ID)
		: base(parent, name, ID)
	{
	}

	public override object GetValue()
	{
		return value;
	}

	public override void BindTo(ValueOutput source)
	{
		if (source is ValueOutput<T>)
		{
			getter = (source as ValueOutput<T>).getter;
		}
		else
		{
			getter = TypeConverter.GetConverterFuncFromTo<T>(source.type, typeof(T), source.GetValue);
		}
	}

	public void BindTo(ValueHandler<T> getter)
	{
		this.getter = getter;
	}

	public override void UnBind()
	{
		getter = null;
	}

	public static explicit operator T(ValueInput<T> port)
	{
		return port.value;
	}
}
