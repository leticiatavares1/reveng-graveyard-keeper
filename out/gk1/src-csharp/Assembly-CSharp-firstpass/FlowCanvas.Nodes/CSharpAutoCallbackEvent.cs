using System;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Automatically Subscribes to the target C# Event when the graph is enabled, and is called when the event is raised")]
[Name("C# Event", 0)]
[DoNotList]
public class CSharpAutoCallbackEvent : EventNode
{
	[SerializeField]
	private SerializedEventInfo _event;

	private ReflectedDelegateEvent reflectedEvent;

	private ValueInput instancePort;

	private FlowOutput callback;

	private object instance;

	private object[] args;

	private EventInfo eventInfo
	{
		get
		{
			if (_event == null)
			{
				return null;
			}
			return _event.Get();
		}
	}

	private bool isStaticEvent
	{
		get
		{
			if (!(eventInfo != null))
			{
				return false;
			}
			return eventInfo.IsStatic();
		}
	}

	public override string name
	{
		get
		{
			if (eventInfo != null && isStaticEvent)
			{
				return $"{base.name} ({eventInfo.RTReflectedType().FriendlyName()})";
			}
			return base.name;
		}
	}

	public void SetEvent(EventInfo info, object instance = null)
	{
		_event = new SerializedEventInfo(info);
		GatherPorts();
	}

	protected override void RegisterPorts()
	{
		if (eventInfo == null)
		{
			return;
		}
		Type eventHandlerType = eventInfo.EventHandlerType;
		if (reflectedEvent == null)
		{
			reflectedEvent = new ReflectedDelegateEvent(eventHandlerType);
		}
		if (!isStaticEvent)
		{
			instancePort = AddValueInput(eventInfo.RTReflectedType().FriendlyName(), eventInfo.RTReflectedType(), "Instance");
		}
		ParameterInfo[] array = eventHandlerType.RTGetDelegateTypeParameters();
		args = new object[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			int i = j;
			ParameterInfo parameterInfo = array[i];
			AddValueOutput(parameterInfo.Name, "arg" + i, parameterInfo.ParameterType, () => args[i]);
		}
		callback = AddFlowOutput(eventInfo.Name, "Event");
	}

	public override void OnGraphStarted()
	{
		if (eventInfo == null)
		{
			return;
		}
		instance = null;
		if (!isStaticEvent)
		{
			instance = instancePort.value;
			if (instance == null)
			{
				Fail("Target is null");
				return;
			}
		}
		eventInfo.AddEventHandler(instance, reflectedEvent.AsDelegate());
		reflectedEvent.Add(OnEventRaised);
	}

	public override void OnGraphStoped()
	{
		if (eventInfo != null)
		{
			eventInfo.RemoveEventHandler(instance, reflectedEvent.AsDelegate());
			reflectedEvent.Remove(OnEventRaised);
		}
	}

	private void OnEventRaised(params object[] args)
	{
		this.args = args;
		callback.Call(default(Flow));
	}
}
