using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Functions")]
[Color("000000")]
[Name("Debug Log WGO", 0)]
public class Flow_DebugLogWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		AddValueInput<string>("Description");
		AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			flow_out.Call(f);
		});
	}
}
