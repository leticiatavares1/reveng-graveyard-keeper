using UnityEngine;

public class Projectile_HitCombat : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponent<ProjectileObjectPart>().on_hit_combat.TryInvoke();
	}
}
