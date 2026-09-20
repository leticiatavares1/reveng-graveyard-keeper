using System;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace FlowCanvas.Nodes;

[Description("Register a callback on a UnityEvent.\nWhen that event is raised, this node will get called.")]
[ContextDefinedInputs(new Type[] { typeof(UnityEventBase) })]
[Category("Events/Custom")]
[Name("Unity Event Callback", 3)]
public class UnityEventCallbackEvent : EventNode
{
	[SerializeField]
	private bool _autoHandleRegistration;

	[SerializeField]
	private SerializedTypeInfo _type;

	private object[] argValues;

	private ValueInput eventInput;

	private FlowOutput callback;

	private ReflectedUnityEvent reflectedEvent;

	private Type eventType
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

	public bool autoHandleRegistration
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
		if (autoHandleRegistration && eventInput.value is UnityEventBase targetEvent)
		{
			reflectedEvent.StartListening(targetEvent, OnEventRaised);
		}
	}

	public override void OnGraphStoped()
	{
		if (autoHandleRegistration && eventInput.value is UnityEventBase targetEvent)
		{
			reflectedEvent.StopListening(targetEvent, OnEventRaised);
		}
	}

	protected override void RegisterPorts()
	{
		eventType = ((eventType != null) ? eventType : typeof(UnityEventBase));
		eventInput = AddValueInput("Event", eventType);
		if (eventType == typeof(UnityEventBase))
		{
			return;
		}
		if (reflectedEvent == null)
		{
			reflectedEvent = new ReflectedUnityEvent(eventType);
		}
		if (reflectedEvent.eventType != eventType)
		{
			reflectedEvent.InitForEventType(eventType);
		}
		argValues = new object[reflectedEvent.parameters.Length];
		for (int j = 0; j < reflectedEvent.parameters.Length; j++)
		{
			int i = j;
			ParameterInfo parameterInfo = reflectedEvent.parameters[i];
			AddValueOutput(parameterInfo.Name, "arg" + i, parameterInfo.ParameterType, () => argValues[i]);
		}
		callback = AddFlowOutput("Callback");
		if (!autoHandleRegistration)
		{
			AddFlowInput("Register", Register, "Add");
			AddFlowInput("Unregister", Unregister, "Remove");
		}
	}

	private void Register(Flow f)
	{
		if (eventInput.value is UnityEventBase targetEvent)
		{
			reflectedEvent.StopListening(targetEvent, OnEventRaised);
			reflectedEvent.StartListening(targetEvent, OnEventRaised);
		}
	}

	private void Unregister(Flow f)
	{
		if (eventInput.value is UnityEventBase targetEvent)
		{
			reflectedEvent.StopListening(targetEvent, OnEventRaised);
		}
	}

	private void OnEventRaised(params object[] args)
	{
		argValues = args;
		callback.Call(default(Flow));
	}

	public override Type GetNodeWildDefinitionType()
	{
		return typeof(UnityEventBase);
	}

	public override void OnPortConnected(Port port, Port otherPort)
	{
		if (port == eventInput && otherPort.type.RTIsSubclassOf(typeof(UnityEventBase)))
		{
			eventType = otherPort.type;
			GatherPorts();
		}
	}
}
