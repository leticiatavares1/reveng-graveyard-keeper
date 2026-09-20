using JetBrains.Annotations;
using UnityEngine;

public class MobAttackSMB : StateMachineBehaviour
{
	private AnimationComponentBase animationComponent;

	[CanBeNull]
	private AttackComponent attackComponent;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (!animationComponent)
		{
			animationComponent = animator.GetComponentInParent<AnimationComponentBase>();
		}
		if (animationComponent != null)
		{
			if (!attackComponent)
			{
				attackComponent = animationComponent.GetComponentInParent<AttackComponent>();
			}
			if ((bool)attackComponent && attackComponent.IsInitialized)
			{
				attackComponent.OnAttackAnimFinished(animationComponent);
			}
		}
	}
}
