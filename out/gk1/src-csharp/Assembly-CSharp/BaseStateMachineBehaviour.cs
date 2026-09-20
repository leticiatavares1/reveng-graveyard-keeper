using UnityEngine;

public class BaseStateMachineBehaviour : StateMachineBehaviour
{
	protected Animator cached_animator;

	protected AnimatedBehaviour behaviour;

	protected float normalized_time;

	protected bool waiting_first_loop;

	protected bool behaviour_cached;

	protected int loop_count;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (behaviour_cached || CheckAnimator(animator))
		{
			behaviour.OnEnter();
		}
		normalized_time = 0f;
		loop_count = 0;
		waiting_first_loop = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (behaviour_cached || CheckAnimator(animator))
		{
			float num = Mathf.Repeat(stateInfo.normalizedTime, 1f);
			int num2 = Mathf.FloorToInt(stateInfo.normalizedTime);
			if (num < normalized_time || (num * stateInfo.length).EqualsTo(stateInfo.length, Time.deltaTime) || num2 > loop_count)
			{
				normalized_time = 0f;
				loop_count++;
				OnLoop();
			}
			else
			{
				normalized_time = num;
			}
			behaviour.OnUpdate(normalized_time);
		}
	}

	protected virtual void OnLoop()
	{
		behaviour.OnLoop();
		if (waiting_first_loop)
		{
			behaviour.OnFirstLoop();
			waiting_first_loop = false;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (behaviour_cached || CheckAnimator(animator))
		{
			behaviour.OnExit();
		}
		if (waiting_first_loop)
		{
			OnLoop();
		}
	}

	protected bool CheckAnimator(Animator animator)
	{
		if (cached_animator != animator)
		{
			cached_animator = animator;
			WorldGameObject worldGameObject = animator.GetComponent<WorldGameObject>() ?? animator.GetComponentInParent<WorldGameObject>() ?? animator.GetComponentInChildren<WorldGameObject>();
			behaviour = worldGameObject.components.animated_behaviour;
		}
		behaviour_cached = behaviour != null;
		return behaviour_cached;
	}
}
