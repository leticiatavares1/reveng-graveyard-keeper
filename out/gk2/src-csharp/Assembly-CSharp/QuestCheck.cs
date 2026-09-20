using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestCheck : IEventTrigerrable
{
	public enum RunModificator
	{
		None,
		TriggerSolo
	}

	public string questId;

	public GlobalEventsSystem.Event.Type triggerType;

	public string triggerId;

	public bool hasTrigger;

	public List<LazyExpression> condExpressions = new List<LazyExpression>();

	public RunModificator runModificator;

	[SerializeField]
	private SGuid uniqueId;

	public string TriggerableId => triggerId;

	public GlobalEventsSystem.Event.Type Type => triggerType;

	public SGuid UniqueId
	{
		get
		{
			return uniqueId;
		}
		set
		{
			uniqueId = value;
		}
	}

	public bool OnTriggerPassed()
	{
		foreach (LazyExpression condExpression in condExpressions)
		{
			if (!condExpression.EvaluateBool())
			{
				return false;
			}
		}
		return true;
	}
}
