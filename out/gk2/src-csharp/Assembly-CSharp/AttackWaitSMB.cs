using JetBrains.Annotations;
using UnityEngine;

public class AttackWaitSMB : StateMachineBehaviour
{
	private AnimationComponentBase animationComponent;

	[CanBeNull]
	private AttackComponent attackComponent;

	private bool wasInterrupted;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!animationComponent)
		{
			animationComponent = animator.GetComponentInParent<AnimationComponentBase>();
		}
		if (animationComponent is PlayerAnimation playerAnimation && MainGame.PlayerController.PlayerInputHandler.meleeMode == MeleeMode.Continuous)
		{
			if (!attackComponent)
			{
				attackComponent = playerAnimation.GetComponentInParent<AttackComponent>();
			}
			attackComponent.OnAttackAnimFinished(animationComponent);
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
	}
}
