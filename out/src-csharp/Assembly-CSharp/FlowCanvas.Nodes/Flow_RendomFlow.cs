using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Random Flow")]
[Name("Random Flow", 0)]
public class Flow_RendomFlow : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<float> in_chance = AddValueInput<float>("chance");
		FlowOutput flow_rolled = AddFlowOutput("true");
		FlowOutput flow_not_rolled = AddFlowOutput("false");
		AddFlowInput("In", delegate(Flow f)
		{
			if (UnityEngine.Random.Range(0f, 1f) < in_chance.value)
			{
				flow_rolled.Call(f);
			}
			else
			{
				flow_not_rolled.Call(f);
			}
		});
	}
}
