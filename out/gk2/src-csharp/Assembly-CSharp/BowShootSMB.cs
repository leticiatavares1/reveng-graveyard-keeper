using System;
using JetBrains.Annotations;
using UnityEngine;

public class BowShootSMB : StateMachineBehaviour
{
	private static readonly int BowPrepare = Animator.StringToHash("bow_preparing");

	private static readonly int BowShoot = Animator.StringToHash("bow_shoot");

	[CanBeNull]
	private AttackComponent attackComponent;

	private AnimationComponentBase animationComponent;

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
			if (!attackComponent)
			{
				attackComponent = playerAnimation.GetComponentInParent<AttackComponent>();
			}
			Action action = (flag ? ((Action)delegate
			{
				animator.SetBool(BowPrepare, value: false);
			}) : null);
			AttackComponent obj = attackComponent;
			if ((object)obj != null)
			{
				Action onAnimationFinished = action;
				obj.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: false, onAnimationFinished);
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
		animator.SetBool(BowShoot, value: false);
	}
}
