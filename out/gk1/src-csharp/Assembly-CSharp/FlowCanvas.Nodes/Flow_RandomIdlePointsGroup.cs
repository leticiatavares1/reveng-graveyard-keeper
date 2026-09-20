using System;
using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Random Idle Points Group")]
[Category("Game Actions")]
[Name("Random Idle Points Group", 0)]
public class Flow_RandomIdlePointsGroup : MyFlowNode
{
	protected override void RegisterPorts()
	{
		GDPoint.IdlePointPrefix random_idle_prefix = GDPoint.IdlePointPrefix.None;
		AddValueOutput("random prefix", () => random_idle_prefix);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			int num = (int)Enum.GetValues(typeof(GDPoint.IdlePointPrefix)).Cast<GDPoint.IdlePointPrefix>().Max();
			random_idle_prefix = (GDPoint.IdlePointPrefix)UnityEngine.Random.Range(0, num - 1);
			flow_out.Call(f);
		});
	}
}
