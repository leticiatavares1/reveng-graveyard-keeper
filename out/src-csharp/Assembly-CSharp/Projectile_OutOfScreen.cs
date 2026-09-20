using UnityEngine;

public class Projectile_OutOfScreen : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponent<ProjectileObjectPart>().on_out_of_screen.TryInvoke();
	}
}
