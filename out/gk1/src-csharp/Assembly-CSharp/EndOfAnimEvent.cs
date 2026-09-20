using UnityEngine;

public class EndOfAnimEvent : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		GUIElements.me.fishing.taking_out_animation_finished = true;
	}
}
