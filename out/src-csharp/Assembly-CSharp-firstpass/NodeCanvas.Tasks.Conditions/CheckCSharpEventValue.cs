using System;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("✫ Script Control/Common")]
[Description("Will subscribe to a public event of Action<T> type and return true when the event is raised and it's value is equal to provided value as well.\n(eg public event System.Action<T> [name])")]
public class CheckCSharpEventValue<T> : ConditionTask
{
	[SerializeField]
	private Type targetType;

	[SerializeField]
	private string eventName;

	[SerializeField]
	private BBParameter<T> checkValue;

	public override Type agentType => targetType ?? typeof(Transform);

	protected override string info
	{
		get
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return "No Event Selected";
			}
			return $"'{eventName}' Raised && Value == {checkValue}";
		}
	}

	protected override string OnInit()
	{
		if (eventName == null)
		{
			return "No Event Selected";
		}
		EventInfo eventInfo = agentType.RTGetEvent(eventName);
		if (eventInfo == null)
		{
			return "Event was not found";
		}
		Delegate handler = GetType().RTGetMethod("Raised").RTCreateDelegate(eventInfo.EventHandlerType, this);
		eventInfo.AddEventHandler(base.agent, handler);
		return null;
	}

	public void Raised(T eventValue)
	{
		if (object.Equals(checkValue.value, eventValue))
		{
			YieldReturn(value: true);
		}
	}

	protected override bool OnCheck()
	{
		return false;
	}
}
