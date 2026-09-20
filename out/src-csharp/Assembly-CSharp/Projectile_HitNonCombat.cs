using UnityEngine;

public class Projectile_HitNonCombat : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponent<ProjectileObjectPart>().on_hit_non_combat.TryInvoke();
	}
}
