using System;
using UnityEngine;

[Serializable]
public class FightingAgentCondition : ConditionalDrawerConditionBase
{
	[Tooltip("FightingAgent condition")]
	public FightingAgentConditionType conditionType;

	public override ConditionalEventType EventType => ConditionalEventType.FightingAgentChanged;

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		bool flag = false;
		switch (conditionType)
		{
		case FightingAgentConditionType.IsActive:
			flag = context.IsFightingAgentActive;
			break;
		case FightingAgentConditionType.IsNotActive:
			flag = !context.IsFightingAgentActive;
			break;
		}
		Debug.Log($"Evaluate FightingAgentCondition [{flag}]");
		return conditionType switch
		{
			FightingAgentConditionType.IsActive => context.IsFightingAgentActive, 
			FightingAgentConditionType.IsNotActive => !context.IsFightingAgentActive, 
			_ => false, 
		};
	}
}
