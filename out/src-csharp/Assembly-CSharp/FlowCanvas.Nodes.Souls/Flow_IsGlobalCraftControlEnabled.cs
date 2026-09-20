using ParadoxNotion.Design;

namespace FlowCanvas.Nodes.Souls;

[Category("Game Actions/Souls")]
[Name("Is Global Craft Control Enable", 0)]
public class Flow_IsGlobalCraftControlEnabled : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @true;

	private FlowOutput @false;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", CalculatePoints);
		@true = AddFlowOutput("True");
		@false = AddFlowOutput("False");
	}

	private void CalculatePoints(Flow flow)
	{
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			@true.Call(flow);
		}
		else
		{
			@false.Call(flow);
		}
	}
}
