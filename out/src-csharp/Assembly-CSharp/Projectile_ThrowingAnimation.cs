using UnityEngine;

public class Projectile_ThrowingAnimation : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		ProjectileEmitter component = animator.gameObject.GetComponent<ProjectileEmitter>();
		if (component.on_anim_end != null)
		{
			component.on_anim_end(succeed: true);
		}
	}
}
