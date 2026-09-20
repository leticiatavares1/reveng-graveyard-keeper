using System;
using UnityEngine;

[Serializable]
public class SetAnimationStateAction : ConditionalDrawerActionBase
{
	[Tooltip("The Animator component to control")]
	public Animator animator;

	[Tooltip("Name of parameter to set")]
	public string parameterId;

	[Tooltip("Type of the parameter to set")]
	public AnimationParamType parameterType;

	[Tooltip("Value to set when condition is true")]
	public int stateValueOnTrue;

	[Tooltip("Value to set when condition is false")]
	public int stateValueOnFalse;

	[Tooltip("Do nothing if condition not met")]
	public bool skipOnConditionNotMet;

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		if (CanApplyAnimation() && (conditionMet || !skipOnConditionNotMet))
		{
			int value = (conditionMet ? stateValueOnTrue : stateValueOnFalse);
			switch (parameterType)
			{
			case AnimationParamType.Bool:
				animator.SetBool(parameterId, conditionMet);
				break;
			case AnimationParamType.Integer:
				animator.SetInteger(parameterId, value);
				break;
			}
		}
	}

	public override void Reset(ConditionalDrawerContext context)
	{
		if (CanApplyAnimation())
		{
			animator.SetInteger(parameterId, stateValueOnFalse);
		}
	}

	private bool CanApplyAnimation()
	{
		if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
		{
			return !string.IsNullOrEmpty(parameterId);
		}
		return false;
	}
}
