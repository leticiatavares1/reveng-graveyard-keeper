using UnityEngine;

public class IdleSMB : StateMachineBehaviour
{
	private Wgo wgo;

	private bool isInitialized;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!isInitialized && !wgo)
		{
			wgo = animator.GetComponentInParent<Wgo>();
			isInitialized = true;
		}
		if ((bool)wgo && wgo.Data.wasCustomAnimationFired)
		{
			wgo.UpdateFlag(ChunkingIgnoreType.Animation, newValue: false);
		}
		base.OnStateEnter(animator, stateInfo, layerIndex);
	}
}
