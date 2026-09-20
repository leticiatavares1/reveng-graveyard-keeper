using UnityEngine;

public class TechPointDropOnAnimDisappeared : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponentInParent<TechPointDrop>().DestroyMe();
	}
}
