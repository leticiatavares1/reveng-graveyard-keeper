using UnityEngine;

public class CharacterSelfStateMachineBehaviour : StateMachineBehaviour
{
	private AnimationComponentBase animationComponent;

	private bool hasAnimationComponent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animationComponent = animator.GetComponentInParent<AnimationComponentBase>();
		hasAnimationComponent = animationComponent != null;
		if (hasAnimationComponent)
		{
			MainGame.PlayerController.SetControlTakenType(TakenControlType.BySelf, isEnabled: false);
			base.OnStateEnter(animator, stateInfo, layerIndex);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animationComponent.SetState(AnimationState.Idle);
		MainGame.PlayerController.SetControlTakenType(TakenControlType.BySelf, isEnabled: true);
	}
}
