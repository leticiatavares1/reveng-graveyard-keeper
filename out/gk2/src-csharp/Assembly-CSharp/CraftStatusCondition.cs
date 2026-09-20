using System;
using UnityEngine;

[Serializable]
public class CraftStatusCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The expected craft status")]
	public ExpectedCraftStatus expectedStatus;

	public bool eventOnly;

	public override ConditionalEventType EventType => ConditionalEventType.CraftStatusChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context))
		{
			return context.CraftComponent != null;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		CraftComponentStatus status = context.CraftComponent.Status;
		if (context.CraftComponent.IsDestroyingCraftActive)
		{
			return false;
		}
		if (eventOnly && !context.IsCalledFromEvent)
		{
			return false;
		}
		return expectedStatus switch
		{
			ExpectedCraftStatus.Started => status == CraftComponentStatus.Started, 
			ExpectedCraftStatus.Finished => status == CraftComponentStatus.Finished && !context.CraftComponent.IsRemovingDestroyCraft, 
			ExpectedCraftStatus.ReadyToFinishAutoCraft => status == CraftComponentStatus.ReadyToFinishAutoCraft, 
			_ => false, 
		};
	}
}
