using UnityEngine;

public class FishGfxTransitionStateSMB : StateMachineBehaviour
{
	private FishUnderwaterGfx fishGfx;

	private FishUnderwaterGfx.FishGfxState stateOnEnter;

	[SerializeField]
	[Range(0f, 1f)]
	private float reverseStartNormalizedTime = 1f;

	private bool isReversing;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		isReversing = false;
		if (!fishGfx)
		{
			fishGfx = animator.GetComponentInParent<FishUnderwaterGfx>();
		}
		if ((bool)fishGfx)
		{
			stateOnEnter = fishGfx.FishState;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		stateOnEnter = FishUnderwaterGfx.FishGfxState.None;
		if (isReversing)
		{
			animator.speed = 1f;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if ((bool)fishGfx && stateOnEnter != fishGfx.FishState)
		{
			stateOnEnter = fishGfx.FishState;
			if (!isReversing)
			{
				isReversing = true;
				animator.Play(stateInfo.fullPathHash, layerIndex, Mathf.Clamp01(reverseStartNormalizedTime));
				animator.speed = -1f;
			}
			else
			{
				isReversing = false;
				animator.speed = 1f;
			}
		}
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}
}
