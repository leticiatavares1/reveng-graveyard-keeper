using UnityEngine;

public class LiftCraneIdleSMB : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		Wgo componentInParent = animator.gameObject.GetComponentInParent<Wgo>();
		if (componentInParent == null)
		{
			Debug.LogError("[LiftCraneIdleSMB] No WGO found for crane " + animator.gameObject.name);
		}
		else
		{
			componentInParent.Data.GameResStr.Set("target_storage_wgo", string.Empty);
		}
	}
}
