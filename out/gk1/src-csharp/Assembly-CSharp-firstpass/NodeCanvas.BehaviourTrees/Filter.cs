using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Category("Decorators")]
[Name("Filter", 0)]
[Icon("Filter", false, "")]
[Description("Filters the access of it's child node either a specific number of times, or every specific amount of time. By default the node is 'Treated as Inactive' to it's parent when child is Filtered. Unchecking this option will instead return Failure when Filtered.")]
public class Filter : BTDecorator
{
	public enum FilterMode
	{
		LimitNumberOfTimes,
		CoolDown
	}

	public enum Policy
	{
		SuccessOrFailure,
		SuccessOnly,
		FailureOnly
	}

	public FilterMode filterMode = FilterMode.CoolDown;

	public BBParameter<int> maxCount = 1;

	public BBParameter<float> coolDownTime = 5f;

	public bool inactiveWhenLimited = true;

	public Policy policy;

	private int executedCount;

	private float currentTime;

	public override void OnGraphStarted()
	{
		executedCount = 0;
		currentTime = 0f;
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		switch (filterMode)
		{
		case FilterMode.CoolDown:
			if (currentTime > 0f)
			{
				if (!inactiveWhenLimited)
				{
					return Status.Failure;
				}
				return Status.Optional;
			}
			base.status = base.decoratedConnection.Execute(agent, blackboard);
			if (base.status == Status.Success || base.status == Status.Failure)
			{
				StartCoroutine(Cooldown());
			}
			break;
		case FilterMode.LimitNumberOfTimes:
			if (executedCount >= maxCount.value)
			{
				if (!inactiveWhenLimited)
				{
					return Status.Failure;
				}
				return Status.Optional;
			}
			base.status = base.decoratedConnection.Execute(agent, blackboard);
			if ((base.status == Status.Success && policy == Policy.SuccessOnly) || (base.status == Status.Failure && policy == Policy.FailureOnly) || ((base.status == Status.Success || base.status == Status.Failure) && policy == Policy.SuccessOrFailure))
			{
				executedCount++;
			}
			break;
		}
		return base.status;
	}

	private IEnumerator Cooldown()
	{
		for (currentTime = coolDownTime.value; currentTime > 0f; currentTime -= Time.deltaTime)
		{
			yield return null;
		}
	}
}
