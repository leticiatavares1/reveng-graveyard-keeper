using UnityEngine;

public class ChurchZombieActivitySMB : StateMachineBehaviour
{
	private ChurchZombieActivity churchZombieActivity;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		churchZombieActivity = animator.GetComponent<ChurchZombieActivity>();
		if (churchZombieActivity != null)
		{
			churchZombieActivity.StartAction();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		churchZombieActivity?.StopAction();
	}
}
