using System;
using UnityEngine;

[Serializable]
public class DockPointStatusChangedCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The expected craft status")]
	public ExpectedDockPointStatus expectedStatus;

	public bool eventOnly;

	public override ConditionalEventType EventType => ConditionalEventType.DockPointStatusChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context) && context.WgoData != null)
		{
			return context.WgoData.MainWgoPartData != null;
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
		foreach (DockPointData dockPointData in context.WgoData.MainWgoPartData.DockPointDataList)
		{
			if (dockPointData.IsOccupied)
			{
				result = true;
				break;
			}
		}
		if (expectedStatus == ExpectedDockPointStatus.IsAnyOccupied)
		{
			return result;
		}
		return false;
	}
}
