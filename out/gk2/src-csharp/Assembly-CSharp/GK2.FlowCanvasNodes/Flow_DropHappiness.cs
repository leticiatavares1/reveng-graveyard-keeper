using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Drop Happiness On Wgo", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_DropHappiness : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), Drop);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Drop(Flow flow)
	{
		GetWgoData().DropHappiness();
		@out.Call(flow);
	}
}
