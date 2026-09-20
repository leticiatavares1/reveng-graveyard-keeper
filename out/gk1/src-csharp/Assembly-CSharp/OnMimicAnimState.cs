using UnityEngine;

public class OnMimicAnimState : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		StateAnimationListener[] componentsInParent = animator.gameObject.GetComponentsInParent<StateAnimationListener>(includeInactive: true);
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i].OnExitedState();
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		StateAnimationListener[] componentsInParent = animator.gameObject.GetComponentsInParent<StateAnimationListener>(includeInactive: true);
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i].OnEnteredState();
		}
	}
}
