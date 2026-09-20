using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Set Weather OLD", 0)]
[Category("Game Actions")]
[Description("OLD FUNCTION, DO NOT USE")]
[Color("FF0000")]
public class Flow_SetWeather : MyFlowNode
{
	protected override void RegisterPorts()
	{
		AddValueInput<float>("Rain");
		AddValueInput<float>("Wind");
		AddValueInput<float>("Fog");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			Debug.LogError("Calling outdated flow block: Flow_SetWeather");
			flow_out.Call(f);
		});
	}
}
