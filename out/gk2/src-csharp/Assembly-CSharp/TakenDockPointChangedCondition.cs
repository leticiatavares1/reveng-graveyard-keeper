using System;
using UnityEngine;

[Serializable]
public class TakenDockPointChangedCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The expected parent wgo id")]
	public string takenDockPointParentWgoId;

	public bool eventOnly;

	public override ConditionalEventType EventType => ConditionalEventType.TakenDockPointChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context))
		{
			return context.WgoData != null;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		if (eventOnly && !context.IsCalledFromEvent)
		{
			return false;
		}
		string text = string.Empty;
		if (!SGuid.IsNullOrEmpty(context.WgoData.takenDockPointsParentSGuid))
		{
			text = MainGame.WorldData.GetWgoData(context.WgoData.takenDockPointsParentSGuid)?.id;
		}
		if (!string.IsNullOrEmpty(takenDockPointParentWgoId) && !string.IsNullOrEmpty(text))
		{
			return text == takenDockPointParentWgoId;
		}
		return false;
	}
}
