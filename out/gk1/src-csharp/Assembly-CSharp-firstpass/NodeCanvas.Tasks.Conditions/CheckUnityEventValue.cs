using System;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Events;

namespace NodeCanvas.Tasks.Conditions;

[Description("Will subscribe to a public UnityEvent<T> and return true when that event is raised and it's value is equal to provided value as well.")]
[Category("✫ Script Control/Common")]
public class CheckUnityEventValue<T> : ConditionTask
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
