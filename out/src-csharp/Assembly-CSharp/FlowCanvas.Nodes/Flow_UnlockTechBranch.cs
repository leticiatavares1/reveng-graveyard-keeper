using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Unlock Tech Branch", 0)]
[Category("Game Actions")]
public class Flow_UnlockTechBranch : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<int> in_value = AddValueInput<int>("Branch Type");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.UnlockTechBranch(in_value.value);
			flow_out.Call(f);
		});
	}
}
