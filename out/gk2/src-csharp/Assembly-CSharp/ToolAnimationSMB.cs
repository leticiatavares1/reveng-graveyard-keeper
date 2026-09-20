using UnityEngine;

public class ToolAnimationSMB : StateMachineBehaviour
{
	public ItemType itemType;

	private int startedLoops;

	private int completedLoops;

	private AnimationComponentBase animationComponent;

	private bool hasAnimationComponent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animationComponent == null)
		{
			animationComponent = animator.GetComponentInParent<AnimationComponentBase>();
			hasAnimationComponent = animationComponent != null;
		}
		if (hasAnimationComponent)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);
			startedLoops = 0;
			completedLoops = 0;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (hasAnimationComponent)
		{
			base.OnStateUpdate(animator, stateInfo, layerIndex);
			int num = Mathf.FloorToInt(stateInfo.normalizedTime);
			if (startedLoops <= num)
			{
				StartLoop();
			}
			if (num > completedLoops)
			{
				CompleteLoop();
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (startedLoops > completedLoops)
		{
			CompleteLoop();
		}
		animationComponent = null;
		hasAnimationComponent = false;
	}

	private void StartLoop()
	{
		startedLoops++;
		animationComponent.HandleLoopStarted(this);
	}

	private void CompleteLoop()
	{
		completedLoops++;
		animationComponent.HandleLoopFinished(this);
	}
}
