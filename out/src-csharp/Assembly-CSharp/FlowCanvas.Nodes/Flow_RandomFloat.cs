using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Random Float")]
[Category("Game Actions")]
[Name("Random Float", 0)]
public class Flow_RandomFloat : MyFlowNode
{
	protected override void RegisterPorts()
	{
		float random_number = 0f;
		ValueInput<float> in_min = AddValueInput<float>("min");
		ValueInput<float> in_max = AddValueInput<float>("max");
		AddValueOutput("random", () => random_number);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_min.value > in_max.value)
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
