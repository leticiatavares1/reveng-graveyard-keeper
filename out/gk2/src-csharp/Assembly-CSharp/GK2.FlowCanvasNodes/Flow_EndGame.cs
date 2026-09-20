using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("End Game", 0)]
[Category("Game")]
public class Flow_EndGame : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool shouldGoToMenuOnReturn;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onWindowClosed;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			MainGame.Instance.CompleteGame(shouldGoToMenuOnReturn, delegate
			{
				onWindowClosed.Call(flow);
			});
			@out.Call(flow);
		});
		@out = AddFlowOutput("out".CapitalizeFirst());
		onWindowClosed = AddFlowOutput("onWindowClosed".CapitalizeFirst());
	}
}
