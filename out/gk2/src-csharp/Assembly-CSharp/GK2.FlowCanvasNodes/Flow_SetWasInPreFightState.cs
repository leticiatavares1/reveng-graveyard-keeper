using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Was In PreFight State", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_SetWasInPreFightState : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool wasInPreFightState;

	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetWasInPreFightState);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void SetWasInPreFightState(Flow flow)
	{
		LazySingleton<FightingGameController>.Instance.SetWasInPreFightState(wasInPreFightState);
		@out.Call(flow);
	}
}
