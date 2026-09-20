using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Is Demo", 0)]
[Category("Game")]
[Color("cf35c1")]
public class Flow_IsDemo : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			yes.Call(flow);
		});
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
	}
}
