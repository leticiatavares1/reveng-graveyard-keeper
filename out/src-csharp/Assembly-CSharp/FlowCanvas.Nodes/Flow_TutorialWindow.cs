using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Tutorial window", 0)]
public class Flow_TutorialWindow : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_id = AddValueInput<string>("id");
		FlowOutput flow_on_closed = AddFlowOutput("   On Closed", "On Closed");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.tutorial.Open(in_id.value, delegate
			{
				flow_on_closed.Call(f);
			});
		});
	}
}
