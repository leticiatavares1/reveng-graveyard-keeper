using UnityEngine;

public class OnIntroAnimationFinished : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Intro.OnIntroAnimationFinished();
	}
}
