using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConditionalDrawerRule
{
	[SerializeReference]
	public IConditionalDrawerCondition condition;

	[SerializeReference]
	public List<IConditionalDrawerAction> actions = new List<IConditionalDrawerAction>();

	public ConditionalEventType EventType => condition?.EventType ?? ConditionalEventType.None;

	public void Evaluate(ConditionalDrawerContext context)
	{
		if (condition == null || !condition.IsValid(context))
		{
			return;
		}
		bool conditionMet = condition.Evaluate(context);
		foreach (IConditionalDrawerAction action in actions)
		{
			action?.Execute(context, conditionMet);
		}
	}

	public void Reset(ConditionalDrawerContext context)
	{
		foreach (IConditionalDrawerAction action in actions)
		{
			action?.Reset(context);
		}
	}
}
