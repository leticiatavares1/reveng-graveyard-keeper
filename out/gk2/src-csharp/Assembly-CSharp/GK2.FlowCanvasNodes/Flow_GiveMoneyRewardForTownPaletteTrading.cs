using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Give Money Reward For Town Palette Trading", 0)]
[Category("Game/UI")]
public class Flow_GiveMoneyRewardForTownPaletteTrading : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), delegate
		{
			GetWgoData().AddMoneyToPlayerAndPaletteTradingResult();
		});
		@out = AddFlowOutput("out".CapitalizeFirst());
	}
}
