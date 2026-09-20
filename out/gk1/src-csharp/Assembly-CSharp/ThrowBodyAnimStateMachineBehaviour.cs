using UnityEngine;

public class ThrowBodyAnimStateMachineBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		MainGame.me.player_component.OnThrowBodyAnimationFinished();
	}
}
