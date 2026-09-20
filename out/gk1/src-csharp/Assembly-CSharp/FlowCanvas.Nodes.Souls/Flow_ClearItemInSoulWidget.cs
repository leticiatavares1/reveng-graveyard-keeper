using ParadoxNotion.Design;

namespace FlowCanvas.Nodes.Souls;

[Name("Clear Item In Soul Widget", 0)]
[Category("Game Actions/Souls")]
public class Flow_ClearItemInSoulWidget : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private float _out_gp_value;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", ClearItem);
		@out = AddFlowOutput("Out");
	}

	private void ClearItem(Flow flow)
	{
		GUIElements.me.soul_healer_gui.soul_healing_widget.ClearSoulItemInGUI();
		@out.Call(flow);
	}
}
