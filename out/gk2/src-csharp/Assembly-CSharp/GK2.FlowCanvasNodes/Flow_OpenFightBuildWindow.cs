using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Open Fight Build Window", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_OpenFightBuildWindow : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), OpenFightBuildWindow);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void OpenFightBuildWindow(Flow flow)
	{
		MilitaryBaseData militaryBaseData = MainGame.Instance.GameSave.militaryBaseData;
		LazySingleton<BuildManager>.Instance.TryEnable(GameScene.GetWgoViewGlobal(base.SelfWgoData.UniqueId), militaryBaseData.CreateBaseBuildingsInventory);
		@out.Call(flow);
	}
}
