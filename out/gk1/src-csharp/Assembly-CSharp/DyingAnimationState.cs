using UnityEngine;

public class DyingAnimationState : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		WorldGameObject componentInParent = animator.gameObject.GetComponentInParent<WorldGameObject>();
		if (componentInParent == null)
		{
			Debug.LogError("Not found WGO for " + animator.gameObject.name, animator);
		}
		else
		{
			componentInParent.DoZeroHPActivity();
		}
	}
}
