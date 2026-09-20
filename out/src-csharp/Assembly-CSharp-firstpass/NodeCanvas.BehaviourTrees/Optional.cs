using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Executes the decorated node without taking into account it's return status, thus making it optional to the parent node for whether it returns Success or Failure.\nThis has the same effect as disabling the node, but instead it executes normaly")]
[Icon("UpwardsArrow", false, "")]
[Name("Optional", 0)]
[Category("Decorators")]
public class Optional : BTDecorator
{
	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Optional;
		}
		if (base.status == Status.Resting)
		{
			base.decoratedConnection.Reset();
		}
		base.status = base.decoratedConnection.Execute(agent, blackboard);
		if (base.status != Status.Running)
		{
			return Status.Optional;
		}
		return Status.Running;
	}
}
