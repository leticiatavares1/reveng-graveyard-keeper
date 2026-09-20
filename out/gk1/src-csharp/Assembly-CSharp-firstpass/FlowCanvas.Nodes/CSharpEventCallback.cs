using System;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Providing a C# Event, Register a callback to be called when that event is raised.")]
[ContextDefinedInputs(new Type[] { typeof(SharpEvent) })]
[Category("Events/Custom")]
[Name("C# Event Callback", 2)]
public class CSharpEventCallback : EventNode
{
	[SerializeField]
	private SerializedTypeInfo _type;

	[SerializeField]
	private bool _autoHandleRegistration;

	private ReflectedDelegateEvent reflectedEvent;

	private FlowOutput flowCallback;

	private ValueInput eventInput;

	private object[] args;

	private Type type
	{
		get
		{
			if (_type == null)
			{
				return null;
			}
			return _type.Get();
		}
		set
		{
			if (_type == null || _type.Get() != value)
			{
				_type = new SerializedTypeInfo(value);
			}
		}
	}

	private bool autoHandleRegistration
	{
		get
		{
			return _autoHandleRegistration;
		}
		set
		{
			if (_autoHandleRegistration != value)
			{
				_autoHandleRegistration = value;
				GatherPorts();
			}
		}
	}

	public override void OnGraphStarted()
	{
		if (autoHandleRegistration && eventInput.value is SharpEvent sharpEvent)
		{
			sharpEvent.StartListening(reflectedEvent, Callback);
		}
	}

	public override void OnGraphStoped()
	{
		if (autoHandleRegistration && eventInput.value is SharpEvent sharpEvent)
		{
			sharpEvent.StopListening(reflectedEvent, Callback);
		}
	}

	protected override void RegisterPorts()
	{
		type = ((type != null) ? type : typeof(SharpEvent));
		eventInput = AddValueInput("Event", type);
		if (type == typeof(SharpEvent))
		{
			return;
		}
		Type delegateType = type.RTGetGenericArguments()[0];
		if (reflectedEvent == null)
		{
			reflectedEvent = new ReflectedDelegateEvent(delegateType);
		}
		ParameterInfo[] array = delegateType.RTGetDelegateTypeParameters();
		for (int j = 0; j < array.Length; j++)
		{
			int i = j;
			ParameterInfo parameterInfo = array[i];
			AddValueOutput(parameterInfo.Name, "arg" + i, parameterInfo.ParameterType, () => args[i]);
		}
		flowCallback = AddFlowOutput("Callback");
		if (!autoHandleRegistration)
		{
			AddFlowInput("Register", Register, "Add");
			AddFlowInput("Unregister", Unregister, "Remove");
		}
	}

	private void Register(Flow f)
	{
		if (eventInput.value is SharpEvent sharpEvent)
		{
			sharpEvent.StopListening(reflectedEvent, Callback);
			sharpEvent.StartListening(reflectedEvent, Callback);
		}
	}

	private void Unregister(Flow f)
	{
		if (eventInput.value is SharpEvent sharpEvent)
		{
			sharpEvent.StopListening(reflectedEvent, Callback);
		}
	}

	private void Callback(params object[] args)
	{
		this.args = args;
		flowCallback.Call(default(Flow));
	}

	public override Type GetNodeWildDefinitionType()
	{
		return typeof(SharpEvent);
	}

	public override void OnPortConnected(Port port, Port otherPort)
	{
		if (port == eventInput)
		{
			type = otherPort.type;
			GatherPorts();
		}
	}
}
