using UnityEngine;

public class Projectile_MaxDistReached : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponent<ProjectileObjectPart>().on_max_dist_reached.TryInvoke();
	}
}
