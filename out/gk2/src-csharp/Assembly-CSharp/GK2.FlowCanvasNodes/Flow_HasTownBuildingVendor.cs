using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Has Town Building Vendor", 0)]
public class Flow_HasTownBuildingVendor : GKCustomFlowNode
{
	private ValueOutput<bool> hasLevelUp;

	protected override void RegisterPorts()
	{
		hasLevelUp = AddValueOutput("hasLevelUp".CapitalizeFirst(), () => MainGame.WorldData.GetWgoData(base.SelfWgoData.LinkedToTownBuildingUniqueId).TownBuildingWgoComponent.HasVendor);
	}
}
