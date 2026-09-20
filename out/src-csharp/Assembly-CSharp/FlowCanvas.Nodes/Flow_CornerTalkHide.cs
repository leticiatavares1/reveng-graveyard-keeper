using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Icon("Dialogue", false, "")]
[Name("Corner Talk Hide", 0)]
public class Flow_CornerTalkHide : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.corner_talk.Hide();
			flow_out.Call(f);
		});
	}
}
