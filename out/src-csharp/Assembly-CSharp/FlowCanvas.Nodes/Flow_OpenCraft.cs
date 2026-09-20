using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Open Craft", 0)]
[Category("Game Actions")]
[Description("Open craft GUI")]
public class Flow_OpenCraft : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.OpenCraftGUI(base.wgo);
			flow_out.Call(f);
		});
	}
}
