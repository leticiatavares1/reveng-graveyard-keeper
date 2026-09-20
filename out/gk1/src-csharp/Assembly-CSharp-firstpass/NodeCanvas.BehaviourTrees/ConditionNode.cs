using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Name("Condition", 0)]
[Description("Check a condition and return Success or Failure")]
[Icon("Condition", false, "")]
public class ConditionNode : BTNode, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	[SerializeField]
	private ConditionTask _condition;

	public Task task
	{
		get
		{
			return condition;
		}
		set
		{
			condition = (ConditionTask)value;
		}
	}

	public ConditionTask condition
	{
		get
		{
			return _condition;
		}
		set
		{
			_condition = value;
		}
	}

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (condition != null)
		{
			if (!condition.CheckCondition(agent, blackboard))
			{
				return Status.Failure;
			}
			return Status.Success;
		}
		return Status.Failure;
	}
}
