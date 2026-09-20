using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Is Max Fighter Containers", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_IsMaxFighterContainers : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput trueOut;

	private FlowOutput falseOut;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), IsMaxFighterContainers);
		trueOut = AddFlowOutput("<color=green>✔</color>");
		falseOut = AddFlowOutput("<color=red>✘</color>");
	}

	private void IsMaxFighterContainers(Flow flow)
	{
		if (MainGame.Instance.GameSave.militaryBaseData.IsMaxFighterContainers)
		{
			trueOut.Call(flow);
		}
		else
		{
			falseOut.Call(flow);
		}
	}
}
