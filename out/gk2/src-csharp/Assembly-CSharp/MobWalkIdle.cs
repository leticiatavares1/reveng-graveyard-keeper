using UnityEngine;

public class MobWalkIdle : StateMachineBehaviour
{
	private AnimationComponentBase animationComponent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!animationComponent)
		{
			animationComponent = animator.GetComponentInParent<AnimationComponentBase>();
		}
		if (animationComponent != null)
		{
			animationComponent.StartZombieIdleSound();
		}
	}
}
