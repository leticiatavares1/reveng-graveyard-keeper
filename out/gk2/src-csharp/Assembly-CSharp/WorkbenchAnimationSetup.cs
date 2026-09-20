using System.Collections.Generic;
using UnityEngine;

public static class WorkbenchAnimationSetup
{
	public const string STATE_PARAMETER = "state";

	public const int STATE_IDLE = 0;

	public const int STATE_WORK = 1;

	public static ConditionalDrawerRule CreateCraftAnimationRule(Animator animator)
	{
		CraftStatusCondition condition = new CraftStatusCondition
		{
			expectedStatus = ExpectedCraftStatus.Started
		};
		SetAnimationStateAction item = new SetAnimationStateAction
		{
			animator = animator,
			parameterId = "state",
			stateValueOnTrue = 1,
			stateValueOnFalse = 0
		};
		return new ConditionalDrawerRule
		{
			condition = condition,
			actions = new List<IConditionalDrawerAction> { item }
		};
	}

	public static ConditionalDrawer SetupWorkbenchAnimation(WgoPart wgoPart, Animator animator)
	{
		if (wgoPart == null || animator == null)
		{
			Debug.LogError("[WorkbenchAnimationSetup] WgoPart or Animator is null");
			return null;
		}
		ConditionalDrawer conditionalDrawer = wgoPart.GetComponent<ConditionalDrawer>();
		if (conditionalDrawer == null)
		{
			conditionalDrawer = wgoPart.gameObject.AddComponent<ConditionalDrawer>();
		}
		ConditionalDrawerRule rule = CreateCraftAnimationRule(animator);
		conditionalDrawer.AddRule(rule);
		return conditionalDrawer;
	}

	public static bool ValidateAnimator(Animator animator)
	{
		if (animator == null || animator.runtimeAnimatorController == null)
		{
			return false;
		}
		AnimatorControllerParameter[] parameters = animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.name == "state" && animatorControllerParameter.type == AnimatorControllerParameterType.Int)
			{
				return true;
			}
		}
		return false;
	}
}
