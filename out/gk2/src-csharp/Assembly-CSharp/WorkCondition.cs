using System;
using UnityEngine;

[Serializable]
public class WorkCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The type of work condition to check")]
	public WorkConditionType conditionType;

	public override ConditionalEventType EventType => ConditionalEventType.WorkStateChanged;

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		CraftComponent craftComponent = context.CraftComponent;
		bool flag = false;
		if (craftComponent != null)
		{
			flag = craftComponent.IsDestroyingCraftActive;
		}
		return conditionType switch
		{
			WorkConditionType.IsInWork => context.IsInWork && !flag, 
			WorkConditionType.IsNotInWork => !context.IsInWork || flag, 
			WorkConditionType.HasWorker => context.HasWorker, 
			WorkConditionType.HasNoWorker => !context.HasWorker, 
			_ => false, 
		};
	}
}
