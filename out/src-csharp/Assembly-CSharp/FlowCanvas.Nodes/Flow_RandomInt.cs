using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Random Integer", 0)]
[Category("Game Actions")]
[Description("Random Integer")]
public class Flow_RandomInt : MyFlowNode
{
	protected override void RegisterPorts()
	{
		int random_number = 0;
		ValueInput<int> in_min = AddValueInput<int>("min (inclusive)");
		ValueInput<int> in_max = AddValueInput<int>("max (exclusive)");
		AddValueOutput("random", () => random_number);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_min.value >= in_max.value)
			{
				Debug.LogError("Wrong random bounds: min = " + in_min.value + ", max = " + in_max.value);
			}
			else
			{
				random_number = UnityEngine.Random.Range(in_min.value, in_max.value);
				flow_out.Call(f);
			}
		});
	}
}
