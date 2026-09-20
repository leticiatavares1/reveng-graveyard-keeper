using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Upgrade Fighter Containers", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_UpgradeFighterContainers : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), UpgradeFighterContainers);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void UpgradeFighterContainers(Flow flow)
	{
		MainGame.Instance.GameSave.militaryBaseData.UpgradeFighterContainers();
		@out.Call(flow);
	}
}
