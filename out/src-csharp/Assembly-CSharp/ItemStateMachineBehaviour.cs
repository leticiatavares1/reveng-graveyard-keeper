using UnityEngine;

public class ItemStateMachineBehaviour : BaseStateMachineBehaviour
{
	public ItemDefinition.ItemType item_type;

	public bool failed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (behaviour_cached || CheckAnimator(animator))
		{
			behaviour.OnItemStart(item_type);
		}
	}

	protected override void OnLoop()
	{
		behaviour.OnItemLoop(item_type, failed);
		if (waiting_first_loop)
		{
			behaviour.OnItemFirstLoop(item_type, failed);
		}
		base.OnLoop();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (behaviour_cached || CheckAnimator(animator))
		{
			behaviour.OnItemStop(item_type);
		}
		base.OnStateExit(animator, stateInfo, layerIndex);
	}
}
