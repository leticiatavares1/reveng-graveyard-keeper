using System;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Events;

namespace NodeCanvas.Tasks.Conditions;

[Category("✫ Script Control/Common")]
[Description("Will subscribe to a public UnityEvent and return true when that event is raised.")]
public class CheckUnityEvent : ConditionTask
{
	[SerializeField]
	private Type targetType;

	[SerializeField]
	private string eventName;

	public override Type agentType => targetType ?? typeof(Transform);

	protected override string info
	{
		get
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return "No Event Selected";
			}
			return $"'{eventName}' Raised";
		}
	}

	protected override string OnInit()
	{
		if (eventName == null)
		{
			return "No Event Selected";
		}
		FieldInfo fieldInfo = agentType.RTGetField(eventName);
		if (fieldInfo == null)
		{
			return "Event was not found";
		}
		((UnityEvent)fieldInfo.GetValue(base.agent)).AddListener(Raised);
		return null;
	}

	public void Raised()
	{
		YieldReturn(value: true);
	}

	protected override bool OnCheck()
	{
		return false;
	}
}
[Category("✫ Script Control/Common")]
[Description("Will subscribe to a public UnityEvent<T> and return true when that event is raised.")]
public class CheckUnityEvent<T> : ConditionTask
{
	[SerializeField]
	private Type targetType;

	[SerializeField]
	private string eventName;

	[SerializeField]
	private BBParameter<T> saveAs;

	public override Type agentType => targetType ?? typeof(Transform);

	protected override string info
	{
		get
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return "No Event Selected";
			}
			return $"'{eventName}' Raised";
		}
	}

	protected override string OnInit()
	{
		if (eventName == null)
		{
			return "No Event Selected";
		}
		FieldInfo fieldInfo = agentType.RTGetField(eventName);
		if (fieldInfo == null)
		{
			return "Event was not found";
		}
		((UnityEvent<T>)fieldInfo.GetValue(base.agent)).AddListener(Raised);
		return null;
	}

	public void Raised(T eventValue)
	{
		saveAs.value = eventValue;
		YieldReturn(value: true);
	}

	protected override bool OnCheck()
	{
		return false;
	}
}
