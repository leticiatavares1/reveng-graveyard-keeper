using System;
using UnityEngine;

public class AttackSMB : StateMachineBehaviour
{
	private AnimationComponentBase animationComponent;

	private bool hasAnimationComponent;

	private bool wasInterrupted;

	public static event Action<AnimationComponentBase> OnAttackFinished;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (animationComponent == null)
		{
			animationComponent = animator.GetComponentInParent<AnimationComponentBase>();
			hasAnimationComponent = animationComponent != null;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		wasInterrupted = animator.IsInTransition(layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (hasAnimationComponent)
		{
			if (stateInfo.normalizedTime >= 1f)
			{
				_ = !wasInterrupted;
			}
			else
				_ = 0;
		}
	}
}
