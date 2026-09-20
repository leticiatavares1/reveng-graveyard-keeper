using System;
using UnityEngine;

[Serializable]
public class SetOccupiedDockPointsWgoAnimationStateAction : ConditionalDrawerActionBase
{
	[Tooltip("Animation state set")]
	public AnimationState animationState;

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		if (context.WgoData == null)
		{
			return;
		}
		AnimationState stateToAnimator = (conditionMet ? animationState : AnimationState.Idle);
		foreach (DockPointData dockPointData in context.WgoData.MainWgoPartData.DockPointDataList)
		{
			if (dockPointData.IsOccupied)
			{
				MainGame.WorldData.GetWgoData(dockPointData.OccupiedBy)?.SetStateToAnimator(stateToAnimator);
			}
		}
	}
}
