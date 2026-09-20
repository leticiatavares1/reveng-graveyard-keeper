using UnityEngine;

public class Projectile_Start : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponent<ProjectileObjectPart>().on_start.TryInvoke();
	}
}
