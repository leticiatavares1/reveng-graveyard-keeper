using UnityEngine;

public class AttackWalkSMB : StateMachineBehaviour
{
	private AnimationComponentBase animationComponent;

	private bool hasAnimationComponent;

	private bool wasInterrupted;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		wasInterrupted = animator.IsInTransition(layerIndex);
	}
}
