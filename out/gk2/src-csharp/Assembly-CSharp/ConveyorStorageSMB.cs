using UnityEngine;

public class ConveyorStorageSMB : StateMachineBehaviour
{
	[SerializeField]
	private int animationIdToSet = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Wgo componentInParent = animator.GetComponentInParent<Wgo>(includeInactive: true);
		if (componentInParent != null && animationIdToSet > -1 && componentInParent.Data.GetGameResInt("activateId") != animationIdToSet)
		{
			componentInParent.Data.SetGameRes("activateId", animationIdToSet);
		}
		base.OnStateEnter(animator, stateInfo, layerIndex);
	}
}
