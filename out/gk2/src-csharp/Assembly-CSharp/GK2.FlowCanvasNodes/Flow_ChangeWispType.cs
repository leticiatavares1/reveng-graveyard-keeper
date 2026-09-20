using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Change Wisp Type", 0)]
[Category("Game/Cutscenes")]
[Color("8a8a8a")]
public class Flow_ChangeWispType : GKCustomFlowNode
{
	private static float CHANGE_WISP_TYPE_TIME = 1f;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onAnimFinish;

	private ValueInput<WispType> wispType;

	public override string name => $"Change Wisp Type to {wispType.value}";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeWispType);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onAnimFinish = AddFlowOutput("onAnimFinish".CapitalizeFirst());
		wispType = AddValueInput<WispType>("wispType".CapitalizeFirst());
	}

	private void ChangeWispType(Flow flow)
	{
		MainGame.PlayerController.WispController.ChangeWispType(wispType.value);
		LazyTimer.AddTimer(CHANGE_WISP_TYPE_TIME, delegate
		{
			onAnimFinish.Call(flow);
		});
		@out.Call(flow);
	}
}
