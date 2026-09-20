using JetBrains.Annotations;
using UnityEngine;

public class AttackIdleSMB : StateMachineBehaviour
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
		SSMState curState = MainGame.PlayerController.Ssm.CurState;
		bool flag = curState is AttackSwordFocusedPlayerState || curState is AttackBowFocusedPlayerState;
		if (animationComponent is PlayerAnimation playerAnimation)
		{
			attackComponent = ((attackComponent == null) ? playerAnimation.GetComponentInParent<AttackComponent>() : attackComponent);
			if (!flag && attackComponent != null)
			{
				attackComponent.OnAttackAnimFinished(animationComponent);
			}
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
