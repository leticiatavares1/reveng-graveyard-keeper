using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("StepIterator", false, "")]
[Description("Executes AND immediately returns children node status ONE-BY-ONE. Step Sequencer always moves forward by one and loops it's index")]
[Name("Step Sequencer", 0)]
[Color("bf7fff")]
[Category("Composites")]
public class StepIterator : BTComposite
{
	private int current;

	public override string name => base.name.ToUpper();

	public override void OnGraphStarted()
	{
		current = 0;
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		current %= base.outConnections.Count;
		return base.outConnections[current].Execute(agent, blackboard);
	}

	protected override void OnReset()
	{
		current++;
	}
}
