using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Can Start Fight", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_CatStartFight : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput trueOut;

	private FlowOutput falseOut;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetFightingLevelName);
		trueOut = AddFlowOutput("<color=green>✔</color>");
		falseOut = AddFlowOutput("<color=red>✘</color>");
	}

	private void SetFightingLevelName(Flow flow)
	{
		if (LazySingleton<FightingGameController>.Instance.CanStartFight())
		{
			trueOut.Call(flow);
		}
		else
		{
			falseOut.Call(flow);
		}
	}
}
