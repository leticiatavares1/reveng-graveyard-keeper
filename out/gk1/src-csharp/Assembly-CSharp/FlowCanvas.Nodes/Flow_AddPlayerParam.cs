using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Add Player Param", 0)]
[Category("Game Actions")]
[Description("Add Player Param")]
public class Flow_AddPlayerParam : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_param_name = AddValueInput<string>("Param name");
		ValueInput<float> in_value = AddValueInput<float>("Value");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.AddToParams(in_param_name.value, in_value.value);
			flow_out.Call(f);
		});
	}
}
