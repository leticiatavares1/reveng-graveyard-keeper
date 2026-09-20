using UnityEngine;
using UnityEngine.Animations;

public class SwordHitBoxNotifierSMB : StateMachineBehaviour
{
	[SerializeField]
	private bool isReverseAnimationPlay;

	private HitStatesAccumulator hitStatesAccumulator;

	private HitStatesAccumulator GetHitStatesAccumulator(Animator animator)
	{
		if (hitStatesAccumulator != null)
		{
			return hitStatesAccumulator;
		}
		hitStatesAccumulator = animator.GetComponentInChildren<HitStatesAccumulator>(includeInactive: true);
		return hitStatesAccumulator;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex, controller);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		GetHitStatesAccumulator(animator)?.Clear();
		base.OnStateExit(animator, stateInfo, layerIndex);
	}
}
