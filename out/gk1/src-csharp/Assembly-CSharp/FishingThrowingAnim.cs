using UnityEngine;

public class FishingThrowingAnim : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (GUIElements.me.fishing.is_shown)
		{
			GUIElements.me.fishing.can_take_out = true;
			FishingGUI.DrawCanHookFishHint();
		}
	}
}
