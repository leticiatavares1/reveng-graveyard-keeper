using System;
using UnityEngine;

[Serializable]
public class ApplyVisualStateAction : ConditionalDrawerActionBase
{
	[Tooltip("The ID of the visual state to apply")]
	public string stateId;

	[Tooltip("If true, only applies when condition is false")]
	public bool applyOnFalse;

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		if (!string.IsNullOrEmpty(stateId) && (applyOnFalse ? (!conditionMet) : conditionMet))
		{
			WgoPartState currentWgoPartState = context.WgoPart.CurrentWgoPartState;
			if (context.WgoPart.WgoPartData != null)
			{
				int rotationIndex = currentWgoPartState?.rotationIndex ?? (-1);
				context.WgoPart.ApplyWgoPartState(stateId, rotationIndex);
			}
		}
	}
}
