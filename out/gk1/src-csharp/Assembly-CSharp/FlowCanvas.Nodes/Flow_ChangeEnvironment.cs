using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Change Environment", 0)]
[Category("Game Actions")]
public class Flow_ChangeEnvironment : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> teleport_tag = AddValueInput<string>("teleport tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			EnvironmentEngine.me.ChangeEnvironment(teleport_tag.value);
			flow_out.Call(f);
		});
	}
}
