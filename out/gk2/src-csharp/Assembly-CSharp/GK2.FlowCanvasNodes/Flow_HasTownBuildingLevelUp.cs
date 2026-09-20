using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Has Town Building LevelUp", 0)]
public class Flow_HasTownBuildingLevelUp : GKCustomFlowNode
{
	private ValueOutput<bool> hasLevelUp;

	private ValueOutput<SmartRes> upgradeLock;

	protected override void RegisterPorts()
	{
		hasLevelUp = AddValueOutput("hasLevelUp".CapitalizeFirst(), delegate
		{
			WgoData wgoData2 = MainGame.WorldData.GetWgoData(base.SelfWgoData.LinkedToTownBuildingUniqueId);
			return wgoData2.TownBuildingWgoComponent.HasLevelUp && !wgoData2.CraftComponent.IsStarted;
		});
		upgradeLock = AddValueOutput("upgradeLock".CapitalizeFirst(), delegate
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(base.SelfWgoData.LinkedToTownBuildingUniqueId);
			if (wgoData.TownBuildingWgoComponent.HasLevelUp && !wgoData.CraftComponent.IsStarted)
			{
				TownBuildingDef data = GameBalance.Me.GetData<TownBuildingDef>(wgoData.TownBuildingWgoComponent.TownBuildingDef.lvlUpId);
				if (data.upgradeRequirements.Count <= 0)
				{
					return (SmartRes)null;
				}
				SmartRes smartRes = new SmartRes
				{
					gameRes = new GameRes()
				};
				{
					foreach (ExpressionGameRes upgradeRequirement in data.upgradeRequirements)
					{
						smartRes.gameRes.Add(upgradeRequirement.name, upgradeRequirement.expression.EvaluateFloat());
					}
					return smartRes;
				}
			}
			return (SmartRes)null;
		});
	}
}
