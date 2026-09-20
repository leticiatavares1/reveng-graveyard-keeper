using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Color("b3ff7f")]
[Icon("Condition", false, "")]
[Description("Quick way to execute the left, or the right child node based on a Condition Task evaluation.")]
[Category("Composites")]
public class BinarySelector : BTNode, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	public bool dynamic;

	[SerializeField]
	private ConditionTask _condition;

	private int succeedIndex;

	public override int maxOutConnections => 2;

	public override Alignment2x2 commentsAlignment => Alignment2x2.Right;

	public override string name => base.name.ToUpper();

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

	private ConditionTask condition
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

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (condition == null || base.outConnections.Count < 2)
		{
			return Status.Failure;
		}
		if (dynamic || base.status == Status.Resting)
		{
			int num = succeedIndex;
			succeedIndex = ((!condition.CheckCondition(agent, blackboard)) ? 1 : 0);
			if (succeedIndex != num)
			{
				base.outConnections[num].Reset();
			}
		}
		return base.outConnections[succeedIndex].Execute(agent, blackboard);
	}
}
