using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Is Mercenary Payed", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_IsMercenaryPayed : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput trueOut;

	private FlowOutput falseOut;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), IsMercenaryPayed);
		trueOut = AddFlowOutput("<color=green>✔</color>");
		falseOut = AddFlowOutput("<color=red>✘</color>");
	}

	private void IsMercenaryPayed(Flow flow)
	{
		if (MainGame.Instance.GameSave.militaryBaseData.IsMercenaryPayed)
		{
			trueOut.Call(flow);
		}
		else
		{
			falseOut.Call(flow);
		}
	}
}
