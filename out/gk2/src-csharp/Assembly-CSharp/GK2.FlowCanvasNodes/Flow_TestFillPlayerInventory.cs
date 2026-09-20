using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Test Fill Player Inventory", 0)]
[Category("Game/UI")]
public class Flow_TestFillPlayerInventory : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Show(Flow flow)
	{
		@out.Call(flow);
	}
}
