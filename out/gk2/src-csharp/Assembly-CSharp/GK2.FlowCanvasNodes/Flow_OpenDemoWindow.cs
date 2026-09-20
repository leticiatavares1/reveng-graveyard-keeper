using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Open Demo Window", 0)]
[Category("Game/UI")]
public class Flow_OpenDemoWindow : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Open);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Open(Flow flow)
	{
		LazyUI.GetWindow<UIDemoEndWindow>().Open(null);
		@out.Call(flow);
	}
}
