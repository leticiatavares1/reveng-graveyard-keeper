using System;
using UnityEngine;

[Serializable]
public class ParentCraftStatusCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The expected craft status")]
	public ExpectedParentCraftStatus expectedStatus;

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
		if (eventOnly && !context.IsCalledFromEvent)
		{
			return false;
		}
		bool result = false;
		foreach (SGuid workbenchParent in context.WgoData.WorkbenchParents)
		{
			WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(workbenchParent);
			if (wgoData != null && wgoData.CraftComponent != null && wgoData.CraftComponent.Status == CraftComponentStatus.Started && !wgoData.CraftComponent.IsDestroyingCraftActive)
			{
				result = true;
				break;
			}
		}
		if (expectedStatus == ExpectedParentCraftStatus.Started)
		{
			return result;
		}
		return false;
	}
}
