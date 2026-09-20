using System;
using UnityEngine;

[Serializable]
public class InteractableCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The type of interact state condition to check")]
	public bool isInteractable;

	public override ConditionalEventType EventType => ConditionalEventType.InteractableStateChanged;

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		if (context.WgoData == null)
		{
			return false;
		}
		return isInteractable == context.WgoData.IsInteractable;
	}
}
