using UnityEngine;

public class AttackAnimationState : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		WorldObjectPart component = animator.gameObject.GetComponent<WorldObjectPart>();
		if (component == null)
		{
			Debug.LogError("Non found WOP!");
			return;
		}
		component.parent.components.character.attack.AttackAnimationEnded();
		Debug.Log("[" + component.name + "] Attack animation ended!");
	}
}
