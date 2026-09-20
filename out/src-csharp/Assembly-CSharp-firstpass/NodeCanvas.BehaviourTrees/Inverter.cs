using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Category("Decorators")]
[Icon("Remap", false, "")]
[Description("Inverts Success to Failure and Failure to Success")]
[Name("Invert", 0)]
public class Inverter : BTDecorator
{
	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		base.status = base.decoratedConnection.Execute(agent, blackboard);
		return base.status switch
		{
			Status.Success => Status.Failure, 
			Status.Failure => Status.Success, 
			_ => base.status, 
		};
	}
}
