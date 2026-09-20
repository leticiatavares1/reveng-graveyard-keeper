using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Body Arrived Notify", 0)]
[Category("Game Actions")]
public class Flow_BodyArrivedNotify : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.body_arrived_gui.Display();
			flow_out.Call(f);
		});
	}
}
